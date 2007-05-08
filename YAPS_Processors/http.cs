/**
 * Original HTTP Server sourcecode:
 * (c) 2001 Sam Pullara  sam@sampullara.com
 * Updated HTTP Server sourcecode+multicast additions:
 * (c) 2006 Daniel Kirstenpfad btk@technology-ninja.com
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Web;
using System.Xml.Serialization;

namespace YAPS
{
    #region HttpProcessor
    /// <summary>
    /// Implements a Handler for each HTTP Client Request
    /// </summary>
    public class HttpProcessor
    {
        #region Variables'r'us
        private static int threads = 0;
		private Socket s;
        public HttpServer HTTPServer;
		public NetworkStream ns;
		private StreamReader sr;
		private StreamWriter sw;
		private string method;
		private string url;
        private string original_url;
        private string querystring;
		private string protocol;
		private Hashtable headers;
		private string request;
		private bool keepAlive = false;
		private int numRequests = 0;
		private bool verbose = HttpServer.verbose;
        private byte[] bytes = new byte[20480];
		private FileInfo docRootFile;
        #endregion

        #region Constructor
        /// <summary>
        /// Each HTTP processor object handles one client.  If Keep-Alive is enabled then this
        /// object will be reused for subsequent requests until the client breaks keep-alive.
        /// This usually happens when it times out.  Because this could easily lead to a DoS
        /// attack, we keep track of the number of open processors and only allow 100 to be
        /// persistent active at any one time.  Additionally, we do not allow more than 500
        /// outstanding requests.
        /// </summary>
        /// <param name="docRoot">Root-Directory of the HTTP Server</param>
        /// <param name="s">the Socket to work with</param>
        /// <param name="webserver">the "master" HttpServer Object of this Client</param>
		public HttpProcessor(string docRoot, Socket s, HttpServer webserver) {
			this.s = s;
			docRootFile = new FileInfo(docRoot);
			headers = new Hashtable();
            HTTPServer = webserver;
        }
        #endregion

        #region processor starting point
        /// <summary>
        /// This is the main method of each thread of HTTP processing.  We pass this method
        /// to the thread constructor when starting a new connection.
        /// </summary>
		public void process() {
			try 
			{
                // Increment the number of current connections
				Interlocked.Increment(ref threads);
				// Bundle up our sockets nice and tight in various streams
				ns = new NetworkStream(s, FileAccess.ReadWrite);
				// It looks like these streams buffer
				sr = new StreamReader(ns);
				sw = new StreamWriter(ns);
				// Parse the request, if that succeeds, read the headers, if that
				// succeeds, then write the given URL to the stream, if possible.
				while (parseRequest())
				{
					if (readHeaders()) {
						// This makes sure we don't have too many persistent connections and also
						// checks to see if the client can maintain keep-alive, if so then we will
						// keep this http processor around to process again.
						if (threads <= 100 && "Keep-Alive".Equals(headers["Connection"]))
						{
							keepAlive = true;
						}
						// Copy the file to the socket
						writeURL();
						// If keep alive is not active then we want to close down the streams
						// and shutdown the socket
						if (!keepAlive)
						{
							ns.Close();
                            try
                            {
                                s.Shutdown(SocketShutdown.Both);
                            }
                            catch (Exception) { }
							break;
						}
					}
				}
			} finally {
				// Always decrement the number of connections
				Interlocked.Decrement(ref threads);	
			}
        }
        #endregion

        #region Validationchecks
        /// <summary>
        /// parses the Request and determines if it's actually a valid one or not
        /// </summary>
        /// <returns>is valid request(true) or not(false)</returns>
        public bool parseRequest() {
			// The number of requests handled by this persistent connection
			numRequests++;
			// Here is where we ensure that we are not overloaded
			if (threads > 500) {
				writeError(502, "Server temporarily overloaded");
				return false;
			}
			// FIXME: This could conceivably used to DoS us if we never finish reading the
			// line and they never hang up.  We could set the socket options to limit
			// the amount of time before reading a request.
			try 
			{
				request = null;
				request = sr.ReadLine();
			} 
			catch (IOException) 
			{
			}
			// If the request line is null, then the other end has hung up on us.  A well
			// behaved client will do this after 15-60 seconds of inactivity.
			if (request == null) {
				if (verbose) {
					ConsoleOutputLogger.WriteLine("Keep-alive broken after " + numRequests + " requests");
				}
				return false;
			}
			// HTTP request lines are of the form:
			// [METHOD] [Encoded URL] HTTP/1.?
			string[] tokens = request.Split(new char[]{' '});
			if (tokens.Length != 3) {
				writeError(400, "Bad request");
				return false;
			}
			// We currently only handle GET requests
			method = tokens[0];
			if(!method.Equals("GET")) {
				writeError(501, method + " not implemented");
				return false;
			}
			url = tokens[1];
			// Only accept valid urls
			if (!url.StartsWith("/")) {
				writeError(400, "Bad URL");
				return false;
			}
			// Decode all encoded parts of the URL using the built in URI processing class			
            // this is buggy
            /*int i = 0;
            while((i = url.IndexOf("%", i)) != -1)
			{
				url = url.Substring(0, i) + Uri.HexUnescape(url, ref i) + url.Substring(i);
			}*/
            
            // this works
            original_url = url;
            url = HttpUtility.UrlDecode(url , Encoding.UTF8);

			// Lets just make sure we are using HTTP, thats about all I care about
			protocol = tokens[2];
			if (!protocol.StartsWith("HTTP/")) {
				writeError(400, "Bad protocol: " + protocol);
			}
			return true;
		}

        /// <summary>
        /// Reads the headers of the request and determines if they are valid or not
        /// </summary>
        /// <returns>is valid request(true) or not(false)</returns>
		public bool readHeaders() {
			string line;
			string name = null;
			// The headers end with either a socket close (!) or an empty line
			while((line = sr.ReadLine()) != null && line != "") {
				// If the value begins with a space or a hard tab then this
				// is an extension of the value of the previous header and
				// should be appended
				if (name != null && Char.IsWhiteSpace(line[0])) {
					headers[name] += line;
					continue;
				}
				// Headers consist of [NAME]: [VALUE] + possible extension lines
				int firstColon = line.IndexOf(":");
				if (firstColon != -1) {
					name = line.Substring(0, firstColon);
					String value = line.Substring(firstColon + 1).Trim();
					if (verbose) ConsoleOutputLogger.WriteLine(name + ": " + value);
					headers[name] = value;
				} else {
					writeError(400, "Bad header: " + line);
					return false;
				}
			}
			return line != null;
        }
        #endregion

        #region HTML OK Message Generator
        String ForwardToIndexHTML(String Message)
        {
            String Output = "";

            Output = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><html><head><title>"+Message+"</title><meta http-equiv=\"refresh\" content=\"5; URL=/index.html\"></head><body><a href=\"/index.html\">"+Message+"</a></body></html>";

            return Output;
        }
        String ForwardToLastPage(String Message)
        {
            String Output = "";

            Output = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><html><head><title>" + Message + "</title><meta http-equiv=\"refresh\" content=\"5; URL=\"javascript:history.back(2)\"></head><body><a href=\"/index.html\">" + Message + "</a></body></html>";

            return Output;
        }
        String ForwardToPage(String Message,String URL)
        {
            String Output = "";

            Output = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\"><html><head><title>" + Message + "</title><meta http-equiv=\"refresh\" content=\"5; URL="+URL+"\"></head><body><a href=\""+URL+"\">" + Message + "</a></body></html>";

            return Output;
        }
        #endregion

        #region GetFileExtension
        /// <summary>
        /// extracts the file extension from a filename
        /// </summary>
        /// <param name="filename">the complete filename or path+filename</param>
        /// <returns>extension as string</returns>
        public string getFileExtension(string filename)
        {
            int position = filename.LastIndexOf('.');

            if (position != -1)
            {
                string output = filename.Remove(0, position);
                return output;
            }
            else
                return "";
        }
        #endregion

        #region Request URL handling
        /// <summary>
        /// We need to make sure that the url that we are trying to treat as a file
        /// lies below the document root of the http server so that people can't grab
        /// random files off your computer while this is running.
        /// </summary>
		public void writeURL() {
			try {
                // first check if the request is actually authenticated

                IPEndPoint AC_endpoint = (IPEndPoint)s.RemoteEndPoint;

                if (!HTTPAuthProcessor.AllowedToAccessThisServer(AC_endpoint.Address))
                {
                    // now give the user a 403 and break...
                    writeForbidden();
                    ns.Flush();
                    return;
                }

                querystring = "";
                url = original_url;
                ConsoleOutputLogger.WriteLine("Request: " + HTTPAuthProcessor.IPtoUsername(AC_endpoint.Address.ToString()) + " - " + url);
                if (url.StartsWith("/vcr/"))
                {
                    #region VCR request
                    // remove the /vcr/ stuff
                    url = url.Remove(0,5);

                    // set this to true when implementing and reaching a new method
                    bool method_found = false;

                    // check which function is called
                        #region AddSearchterm
                        if (url.ToUpper().StartsWith("ADDSEARCHTERM"))
                        {
                            method_found = true;
                            url = url.Remove(0, 14);

                            // let's check for the category

                            // TODO: better Querystring Handling!!!!
                            
                            string[] splitted = url.Split('&');

                            string categoryname = "";
                            string newsearchterm = "";

                            string[] splitted2 = splitted[0].Split('=');
                            string[] splitted3 = splitted[1].Split('=');

                            if (splitted2[0].ToUpper() == "CATEGORY")
                            {
                                categoryname = splitted2[1];
                            }
                            if (splitted3[0].ToUpper() == "NEW_TERM")
                            {
                                newsearchterm = splitted3[1];
                            }

                            if ((newsearchterm != "") & (categoryname != ""))
                            {
                                string Output;

                                if (HTTPServer.vcr_scheduler.Category_Processor.AddSearchTerm(HttpUtility.UrlDecode(categoryname, Encoding.UTF7), HttpUtility.UrlDecode(newsearchterm, Encoding.UTF7)))
                                {
                                    // check if there's already a category with that name
                                    Output = ForwardToPage("Searchterm " + HttpUtility.UrlDecode(newsearchterm, Encoding.UTF7) + " successfully added to the category " + HttpUtility.UrlDecode(categoryname, Encoding.UTF7), "/editcategory_step3.html?category="+categoryname);
                                }
                                else
                                {
                                    Output = ForwardToPage("Error: Searchterm not added...", "/editcategory_step3.html?category=" + categoryname);
                                }
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();
                            }
                            // if not..well forget about it
                        }
                        #endregion

                        #region DelSearchterm
                        if (url.ToUpper().StartsWith("DELSEARCHTERM"))
                        {
                            method_found = true;
                            url = url.Remove(0, 14);

                            // let's check for the category

                            // TODO: better Querystring Handling!!!!

                            string[] splitted = url.Split('&');

                            string categoryname = "";
                            string searchterm = "";

                            string[] splitted2 = splitted[0].Split('=');
                            string[] splitted3 = splitted[1].Split('=');

                            if (splitted2[0].ToUpper() == "CATEGORY")
                            {
                                categoryname = splitted2[1];
                            }
                            if (splitted3[0].ToUpper() == "TERM")
                            {
                                searchterm = splitted3[1];
                            }

                            if ((searchterm != "") & (categoryname != ""))
                            {
                                string Output;

                                if (HTTPServer.vcr_scheduler.Category_Processor.DelSearchTerm(HttpUtility.UrlDecode(categoryname, Encoding.UTF7), HttpUtility.UrlDecode(searchterm, Encoding.UTF7)))
                                {
                                    // check if there's already a category with that name
                                    Output = ForwardToPage("Searchterm " + HttpUtility.UrlDecode(searchterm, Encoding.UTF7) + " successfully deleted from category " + HttpUtility.UrlDecode(categoryname, Encoding.UTF7), "/editcategory_step4.html?category=" + categoryname);
                                }
                                else
                                {
                                    Output = ForwardToPage("Error: Searchterm not deleted...", "/editcategory_step4.html?category=" + categoryname);
                                }
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();
                            }
                            // if not..well forget about it
                        }
                        #endregion

                        #region AddCategory
                        if (url.ToUpper().StartsWith("ADDCATEGORY"))
                        {
                            method_found = true;
                            url = url.Remove(0, 12);

                            string[] splitted = url.Split('=');
                            if (splitted[0].ToUpper() == "CATEGORYNAME")
                            {
                                string Output;
                                // check if there's already a category with that name

                                // it's UTF7 in this case because it comes directly from the browser...
                                if (HTTPServer.vcr_scheduler.Category_Processor.AddCategory(HttpUtility.UrlDecode(splitted[1], Encoding.UTF7)))
                                {
                                    Output = ForwardToPage("Category " + HttpUtility.UrlDecode(splitted[1], Encoding.UTF7) + " successfully added..", "/addcategory.html");
                                }
                                else
                                {
                                    // failed
                                    Output = ForwardToPage("Error: Category not added...", "/addcategory.html");
                                }
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();                           
                            }
                        }
                        #endregion

                        #region DelCategory
                        // rewritten addrecording/deleterecording...
                        if (url.ToUpper().StartsWith("DELCATEGORY"))
                        {
                            method_found = true;
                            url = url.Remove(0, 12);

                            string[] splitted = url.Split('=');
                            if (splitted[0].ToUpper() == "DELCATEGORY")
                            {
                                string Output;
                                // it's UTF7 in this case because it comes directly from the browser...
                                if (HTTPServer.vcr_scheduler.Category_Processor.DelCategory(HttpUtility.UrlDecode(splitted[1], Encoding.UTF7)))
                                {
                                    Output = ForwardToPage("Category " + HttpUtility.UrlDecode(splitted[1], Encoding.UTF7) + " successfully deleted..", "/delcategory.html");
                                }
                                else
                                {
                                    // failed
                                    Output = ForwardToPage("Error: Category not found...", "/delcategory.html");
                                }
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();
                            }
                        }
                        #endregion

                        #region ManageRecording
                        // rewritten addrecording/deleterecording...
                        if (url.ToUpper().StartsWith("MANAGERECORDING"))
                        {
                            url = url.Remove(0, 16);

                            string[] parameterlist = url.Split('&');

                            // what type of function are we calling now?

                            // fix the encoding; UTF8 that is
                            for (int i = 0; i < parameterlist.Length; i++)
                            {
                                string[] split_parameter = HttpUtility.UrlDecode(parameterlist[i], Encoding.UTF8).Split('=');
                                if (split_parameter[0].ToUpper() == "TYPE")
                                {
                                    if (split_parameter[1].ToUpper() == "DEL")
                                    {
                                        url = "DELETERECORDING/" + url;
                                        break;
                                    }
                                    if (split_parameter[1].ToUpper() == "ADD")
                                    {
                                        url = "ADDRECORDING/" + url;
                                        break;
                                    }

                                }
                            }
                        }
                        #endregion

                        #region AddRecording
                        if (url.ToUpper().StartsWith("ADDRECORDING"))
                        {
                            #region CheckAuthentification
                            if (!HTTPAuthProcessor.AllowedToCreateRecordings(AC_endpoint.Address))
                            {
                                // now give the user a 403 and break...
                                writeForbidden();
                                ns.Flush();
                                return;
                            }
                            #endregion

                            method_found = true;
                            url = url.Remove(0, 13);

                            string[] parameterlist = url.Split('&');

                            // fix the encoding; UTF8 that is
                            for (int i = 0; i < parameterlist.Length; i++)
                            {
                                parameterlist[i] = HttpUtility.UrlDecode(parameterlist[i], Encoding.UTF8);
                            }

                            // instantiate new Recording
                            Recording newTimer = new Recording();

                            #region data'n'stuff
                            int s_date = 0, s_month = 0, s_year = 0, s_hour = 0, s_minute = 0, e_date = 0, e_month = 0, e_year = 0, e_hour = 0, e_minute = 0;
                            #endregion

                            // TODO: add some try..catch thingies..
                            foreach (string Parameter in parameterlist)
                            {
                                try
                                {
                                    string[] split_parameter = Parameter.Split('=');

                                    // Zwischenergebniss: s_date=1&s_month=8&s_year=2006&s_hour=12&s_minute=12&e_date=1
                                    // &e_month=8&e_year=2006&e_hour=12&e_minute=12&name=1asdfasdfasf&ch=25

                                    switch (split_parameter[0].ToUpper())
                                    {
                                        case "S_DATE":
                                            s_date = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "S_MONTH":
                                            s_month = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "S_YEAR":
                                            s_year = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "S_HOUR":
                                            s_hour = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "S_MINUTE":
                                            s_minute = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "E_DATE":
                                            e_date = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "E_MONTH":
                                            e_month = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "E_YEAR":
                                            e_year = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "E_HOUR":
                                            e_hour = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "E_MINUTE":
                                            e_minute = Convert.ToInt32(split_parameter[1]);
                                            break;
                                        case "NAME":
                                            newTimer.Recording_Name = HttpUtility.UrlDecode(split_parameter[1], Encoding.UTF8); // UTF-8 Decoding..
                                            break;
                                        case "CH":
                                            newTimer.Channel = HttpUtility.UrlDecode(split_parameter[1], Encoding.UTF8);// UTF-8 Decoding..
                                            break;
                                        default:
                                            // alles andere interessiert uns nicht
                                            break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    ConsoleOutputLogger.WriteLine("AddRecording: "+e.Message);
                                }
                            }
                            // TODO: add more checks for name+channel

                            // we're trying to map the given name to a number...
                            int ChannelNumber = ChannelAndStationMapper.Name2Number(newTimer.Channel);

                            if (ChannelNumber != -1)
                            {
                                ConsoleOutputLogger.WriteLine("Apparently the channel " + newTimer.Channel + " has the number " + Convert.ToString(ChannelNumber));
                                // we found something...
                                newTimer.Channel = Convert.ToString(ChannelNumber);
                            }

                            // try to map the IP to an username
                            newTimer.createdby = HTTPAuthProcessor.IPtoUsername(AC_endpoint.Address.ToString());

                            ConsoleOutputLogger.WriteLine("Apparently the username is " + newTimer.createdby);


                            // TODO: check if there are enough information given to actually create that timer
                            try
                            {
                                newTimer.StartsAt = new DateTime(s_year, s_month, s_date, s_hour, s_minute, 0);
                                newTimer.EndsAt = new DateTime(e_year, e_month, e_date, e_hour, e_minute, 0);
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("AddRecording: " + e.Message);
                            }
                            newTimer.Recording_Filename = newTimer.Recording_ID.ToString();

                            // TODO: check for other timers that could possibly concur...
                            lock (HTTPServer.vcr_scheduler.Recordings.SyncRoot)
                            {
                                HTTPServer.vcr_scheduler.Recordings.Add(newTimer.Recording_ID, newTimer);
                            }

                            // tell the client that everything went fine...
                            string Output = ForwardToIndexHTML("New Timer created...");
                            byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                            int left = new UnicodeEncoding().GetByteCount(Output);
                            writeSuccess(left);
                            ns.Write(buffer, 0, left);
                            ns.Flush();

                            // Save the new Configuration...
                            HTTPServer.Configuration.SaveSettings();
                        }
                        #endregion

                        #region DeleteRecording
                        if (url.ToUpper().StartsWith("DELETERECORDING"))
                        {
                            // TODO: implement correct manage-recording version...
                            method_found = true;
                            // remove the /vcr/deleterecording? stuff
                            url = url.Remove(0, 16);

                            string[] splitted = url.Split('=');
                            if (splitted[0].ToUpper() == "ID")
                            {
                                Guid delete_entry_id = Guid.NewGuid();
                                string recording_name = "";
                                string recording_username = "";

                                lock (HTTPServer.vcr_scheduler.Recordings.SyncRoot)
                                {
                                    foreach (Recording recording_entry in HTTPServer.vcr_scheduler.Recordings.Values)
                                    {
                                        string original = recording_entry.Recording_ID.ToString();

                                        if (original == (splitted[1]))
                                        {
                                            recording_name = recording_entry.Recording_Name;
                                            delete_entry_id = recording_entry.Recording_ID;
                                            recording_username = recording_entry.createdby;
                                        }
                                    }
                                }
                                #region CheckAuthentification
                                if (!HTTPAuthProcessor.AllowedToDeleteRecordings(AC_endpoint.Address,recording_username))
                                {
                                    // now give the user a 403 and break...
                                    writeForbidden();
                                    ns.Flush();
                                    return;
                                }
                                #endregion

                                bool deleted = true;
                                try
                                {
                                    lock (HTTPServer.vcr_scheduler.Recordings.SyncRoot)
                                    {
                                        HTTPServer.vcr_scheduler.Recordings.Remove(delete_entry_id);
                                    }
                                }
                                catch (Exception)
                                {
                                    // failed
                                    deleted = false;
                                }
                                // tell the client that everything went fine...
                                string Output;
                                if (deleted)
                                    Output = ForwardToPage("Recording: " + recording_name + " successfully deleted...","/index.html");
                                else
                                    Output = ForwardToPage("Error: Recording not deleted...","/index.html");
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();

                                // Save the new Configuration...
                                HTTPServer.Configuration.SaveSettings();
                            }
                            else
                            {
                                string[] parameterlist = url.Split('&');

                                // fix the encoding; UTF8 that is
                                for (int i = 0; i < parameterlist.Length; i++)
                                {
                                    parameterlist[i] = HttpUtility.UrlDecode(parameterlist[i], Encoding.UTF8);
                                }

                                // instantiate new Recording
                                Recording newTimer = new Recording();

                                #region data'n'stuff
                                int s_date = 0, s_month = 0, s_year = 0, s_hour = 0, s_minute = 0, e_date = 0, e_month = 0, e_year = 0, e_hour = 0, e_minute = 0;
                                #endregion

                                // find some information to identify the recording and delete it...
                                try
                                {
                                    foreach (string Parameter in parameterlist)
                                    {
                                        string[] split_parameter = Parameter.Split('=');

                                        // Zwischenergebniss: s_date=1&s_month=8&s_year=2006&s_hour=12&s_minute=12&e_date=1
                                        // &e_month=8&e_year=2006&e_hour=12&e_minute=12&name=1asdfasdfasf&ch=25

                                        switch (split_parameter[0].ToUpper())
                                        {
                                            case "S_DATE":
                                                s_date = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "S_MONTH":
                                                s_month = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "S_YEAR":
                                                s_year = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "S_HOUR":
                                                s_hour = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "S_MINUTE":
                                                s_minute = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "E_DATE":
                                                e_date = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "E_MONTH":
                                                e_month = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "E_YEAR":
                                                e_year = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "E_HOUR":
                                                e_hour = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "E_MINUTE":
                                                e_minute = Convert.ToInt32(split_parameter[1]);
                                                break;
                                            case "NAME":
                                                newTimer.Recording_Name = HttpUtility.UrlDecode(split_parameter[1], Encoding.UTF8); // UTF-8 Decoding..
                                                break;
                                            case "CH":
                                                newTimer.Channel = HttpUtility.UrlDecode(split_parameter[1], Encoding.UTF8);// UTF-8 Decoding..
                                                break;
                                            default:
                                                // alles andere interessiert uns nicht
                                                break;
                                        }
                                    }

                                    // we're trying to map the given name to a number...
                                    int ChannelNumber = ChannelAndStationMapper.Name2Number(newTimer.Channel);

                                    if (ChannelNumber != -1)
                                    {
                                        // we found something...
                                        newTimer.Channel = Convert.ToString(ChannelNumber);
                                    }
                                    
                                    try
                                    {
                                        newTimer.StartsAt = new DateTime(s_year, s_month, s_date, s_hour, s_minute, 0);
                                        newTimer.EndsAt = new DateTime(e_year, e_month, e_date, e_hour, e_minute, 0);
                                    }
                                    catch (Exception e)
                                    {
                                        ConsoleOutputLogger.WriteLine("Deleterecording: " + e.Message);
                                    }
                                    Guid tobedeletedID = Guid.NewGuid();
                                    bool foundanID = false;
                                    string recording_username = "";

                                    // now we search for a corresponding recording ...
                                    lock (HTTPServer.vcr_scheduler.Recordings.SyncRoot)
                                    {
                                        foreach (Recording timer in HTTPServer.vcr_scheduler.Recordings.Values)
                                        {
                                            if (timer.Channel == newTimer.Channel)
                                            {
                                                if (timer.StartsAt == newTimer.StartsAt)
                                                {
                                                    if (timer.EndsAt == newTimer.EndsAt)
                                                    {
                                                        // found it...now save the key
                                                        tobedeletedID = timer.Recording_ID;
                                                        foundanID = true;
                                                        recording_username = timer.createdby;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        if (foundanID)
                                        {
                                            #region CheckAuthentification
                                            if (!HTTPAuthProcessor.AllowedToDeleteRecordings(AC_endpoint.Address, recording_username))
                                            {
                                                // now give the user a 403 and break...
                                                writeForbidden();
                                                ns.Flush();
                                                return;
                                            }
                                            #endregion

                                            ConsoleOutputLogger.WriteLine("Found Recording ... deleting");
                                            HTTPServer.vcr_scheduler.Recordings.Remove(tobedeletedID);
                                        }
                                    }
                                    // tell the client that everything went fine...
                                    string Output;
                                    if (foundanID)
                                        Output = ForwardToPage("Recording successfully deleted...","/index.html");
                                    else
                                        Output = ForwardToPage("Error: Recording not deleted...","index.html");
                                    byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                    int left = new UnicodeEncoding().GetByteCount(Output);
                                    writeSuccess(left);
                                    ns.Write(buffer, 0, left);
                                    ns.Flush();
                                }
                                catch (Exception e)
                                {
                                    ConsoleOutputLogger.WriteLine("Deleterecording(Findrecording): " + e.Message);
                                    writeFailure();
                                }
                            }
                        }
                        #endregion

                        #region RemoveRecordingFile
                        if (url.ToUpper().StartsWith("REMOVERECORDINGFILE"))
                        {
                            method_found = true;
                            // remove the deleterecording? stuff
                            url = url.Remove(0, 20);

                            string[] splitted = url.Split('=');
                            if (splitted[0].ToUpper() == "ID")
                            {
                                Guid delete_entry_id = Guid.NewGuid();
                                //String delete_entry_filename = "";

                                Recording tobedeletedrecording = null;

                                lock (HTTPServer.vcr_scheduler.doneRecordings.SyncRoot)
                                {
                                    foreach (Recording recording_entry in HTTPServer.vcr_scheduler.doneRecordings.Values)
                                    {
                                        string original = recording_entry.Recording_ID.ToString();

                                        if (original == splitted[1])
                                        {
                                            tobedeletedrecording = recording_entry;
                                        }
                                    }
                                }
                                bool deleted = true;

                                #region CheckAuthentification
                                if (!HTTPAuthProcessor.AllowedToDeleteRecordings(AC_endpoint.Address, tobedeletedrecording.createdby))
                                {
                                    // now give the user a 403 and break...
                                    writeForbidden();
                                    ns.Flush();
                                    return;
                                }
                                #endregion


                                try
                                {
                                    lock (HTTPServer.vcr_scheduler.doneRecordings.SyncRoot)
                                    {
                                        // remove file
                                        File.Delete(tobedeletedrecording.Recording_Filename);

                                        // remove entry in Hashtable
                                        HTTPServer.vcr_scheduler.doneRecordings.Remove(tobedeletedrecording.Recording_ID);
                                    }
                                }
                                catch (Exception)
                                {
                                    // failed
                                    deleted = false;
                                }
                                // tell the client that everything went fine...
                                string Output;
                                if (deleted)
                                    Output = ForwardToPage("Recording: " + tobedeletedrecording.Recording_Name + " successfully deleted...","/recordings.html");
                                else
                                    Output = ForwardToPage("Error: Recording not deleted...", "/recordings.html");
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();

                                // Remove the playlist file for the XBMC player
                                if (deleted)
                                {
                                    try
                                    {
                                        File.Delete(XBMCPlaylistFilesHelper.generatePlaylistFilename(tobedeletedrecording));
                                        File.Delete(XBMCPlaylistFilesHelper.generateThumbnailFilename(tobedeletedrecording));
                                    }
                                    catch
                                    {
                                        ConsoleOutputLogger.WriteLine("HTTP Server Exception: Could not delete the playlistfile for " + tobedeletedrecording.Recording_Name);
                                    }
                                }

                                // Save the new Configuration...
                                HTTPServer.Configuration.SaveSettings();
                            }
                            else
                                writeFailure();

                        }
                        #endregion

                        #region Reset PlayPosition
                        if (url.ToUpper().StartsWith("RESETRECORDING"))
                        {
                            method_found = true;
                            // remove the deleterecording? stuff
                            url = url.Remove(0, 15);

                            string[] splitted = url.Split('=');
                            if (splitted[0].ToUpper() == "ID")
                            {
                                Guid delete_entry_id = Guid.NewGuid();

                                lock (HTTPServer.vcr_scheduler.doneRecordings.SyncRoot)
                                {
                                    foreach (Recording recording_entry in HTTPServer.vcr_scheduler.doneRecordings.Values)
                                    {
                                        string original = recording_entry.Recording_ID.ToString();

                                        if (original == splitted[1])
                                        {
                                            recording_entry.LastStoppedPosition = 0;
                                        }
                                    }
                                }
                                string Output;
                                Output = ForwardToPage("Recording PlayPosition was reset...", "/recordings.html");
                                byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                                int left = new UnicodeEncoding().GetByteCount(Output);
                                writeSuccess(left);
                                ns.Write(buffer, 0, left);
                                ns.Flush();

                                // Save the new Configuration...
                                HTTPServer.Configuration.SaveSettings();
                            }
                            else
                                writeFailure();

                        }
                        #endregion

                    if (!method_found)
                    {
                        // nothing to do...
                        writeError(404, "No Method found");
                    }
                    #endregion
                }
                else
                if (url.StartsWith("/rss/"))
                {
                    #region XML RSS feed requests
                    // remove the /rss/ stuff
                    url = url.Remove(0, 5);

                    #region Recordings Listing RSS
                    if (url.ToUpper().StartsWith("RECORDED.XML"))
                    {
                        url = url.Remove(0, 12);

                        System.IO.MemoryStream XMLStream = new System.IO.MemoryStream();

                        RSS.RSSChannel oRSSChannel =  new RSS.RSSChannel("Recorded Listings",HTTPServer.Settings.HTTP_URL, "Displays all the recordings currently available for watching");
                        oRSSChannel.PubDate = System.DateTime.Now.ToString();

                        RSS.RSSImage oRSSImage = new RSS.RSSImage(HTTPServer.Settings.HTTP_URL + "/images/RSS_Logo_YAPS.png", HTTPServer.Settings.HTTP_URL, "YAPS VCR");

                        RSS.RSSRoot oRSSRoot = new RSS.RSSRoot(oRSSChannel,oRSSImage, XMLStream);

                        RSS.RSSItem oRSSItem = null;

                        // now go for a walk with all the Recordings...

                        #region Sort the Recordings Listing
                        List<Recording> sortedDoneRecordingList;
                        lock (HTTPServer.vcr_scheduler.doneRecordings.SyncRoot)
                        {
                            // TODO: add ascending/descending setting + reimplement the sorting algorithm
                            sortedDoneRecordingList = Sorter.SortRecordingTable(HTTPServer.vcr_scheduler.doneRecordings, false);
                        }
                        #endregion

                        // TODO: maybe we can do this without foreach...faster; but because this is not critical function of YAPS...well low priority
                        foreach (Recording recording in sortedDoneRecordingList)
                        {
                            oRSSItem = new RSS.RSSItem(recording.Recording_Name, HTTPServer.Settings.HTTP_URL + "/" + recording.Recording_ID.ToString());
                            oRSSItem.PubDate = recording.StartsAt.ToString();
                            oRSSItem.Comment = recording.Comment;

                            // generate Category List for this recording
                            oRSSItem.Categories = HTTPServer.vcr_scheduler.Category_Processor.RenderCategoryLine(HTTPServer.vcr_scheduler.Category_Processor.AutomaticCategoriesForRecording(recording), ',');

                            oRSSRoot.Items.Add(oRSSItem);
                        }

                        RSS.RSSUtilities.PublishRSS(oRSSRoot);

                        XMLStream.Seek(0, SeekOrigin.Begin);

                        byte[] byteArray = new byte[XMLStream.Length];
                        int xmlcount = XMLStream.Read(byteArray, 0, Convert.ToInt32(XMLStream.Length));

                        writeSuccess(xmlcount,"text/xml");
                        ns.Write(byteArray, 0, xmlcount);
                        ns.Flush();
                        
                        XMLStream.Close();
                    }
                    #endregion

                    #endregion
                }
                else
                if (url.StartsWith("/dvb/"))
                {
                    #region Streaming Request

                    #region CheckAuthentification
                    if (!HTTPAuthProcessor.AllowedToAccessLiveStream(AC_endpoint.Address))
                    {
                        // now give the user a 403 and break...
                        writeForbidden();
                        ns.Flush();
                        return;
                    }
                    #endregion

                    // check if there is a MulticastProcessor Object for this channel
                    VCRandStreaming HReq = new VCRandStreaming(false,null,HTTPServer);

                    #region CheckAuthentification
                    if (!HTTPAuthProcessor.AllowedToAccessLiveStream(AC_endpoint.Address))
                    {
                        // now give the user a 403 and break...
                        writeForbidden();
                        ns.Flush();
                        return;
                    }
                    #endregion


                    try
                    {
                        HReq.HandleStreaming(url, this);
                        while (!HReq.done)
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("Streaming error: " + e.Message);
                        try
                        {
                            writeFailure();
                        }
                        catch (Exception)
                        {
                            ConsoleOutputLogger.WriteLine("Streaming error: client disconnected");
                        }
                    }
                    #endregion
                }
                else
                if (url.StartsWith("/xml/"))
                {
                    #region Tuxbox Request
                    #region CheckAuthentification
                    if (!HTTPAuthProcessor.AllowedToAccessTuxbox(AC_endpoint.Address))
                    {
                        // now give the user a 403 and break...
                        writeForbidden();
                        ns.Flush();
                        return;
                    }
                    #endregion

                    url = url.Remove(0,5);

                    // set this to true when implementing and reaching a new method
                    bool method_found = false;

                    // check which function is called

                    #region boxstatus
                    if (url.ToUpper().StartsWith("BOXSTATUS"))
                    {
                        method_found = true;
                        /*
                         * <status>
                         * <current_time>Sat Jul 23 23:03:17 2005</current_time>
                         *  <standby>OFF</standby>
                         *  <recording>OFF</recording>
                         *  <mode>0</mode>
                         *  <ip>192.168.0.10</ip>
                         * </status>
                         * 
                         * */
                        StringBuilder boxstatus = new StringBuilder();

                        // of course it would be possible to do this through serializers but it would be
                        // much more hassle than this:

                        boxstatus.AppendLine("<status>");
                        boxstatus.AppendLine("<current_time>"+DateTime.Now.ToString()+"</current_time>");
                        boxstatus.AppendLine("<standby>OFF</standby>");
                        boxstatus.AppendLine("<recording>OFF</recording>");
                        boxstatus.AppendLine("<mode>0</mode>");
                        boxstatus.AppendLine("<ip>"+HTTPServer.ipaddress.ToString()+"</ip>");
                        boxstatus.AppendLine("</status>");

                        byte[] buffer = new UTF8Encoding().GetBytes(boxstatus.ToString());

                        writeSuccess(buffer.Length, "text/xml");

                        ns.Write(buffer, 0, buffer.Length);
                        ns.Flush();

                    }
                    #endregion

                    #region services
                    if (url.ToUpper().StartsWith("SERVICES"))
                    {
                        method_found = true;

                        System.IO.MemoryStream XMLStream = new System.IO.MemoryStream();
                        // TODO: add check if EPGProcessor is even instantiated

                        try
                        {
                            List<tuxbox.bouquet> bouquets = new List<YAPS.tuxbox.bouquet>();

                            // add Currently Running events to the list...
                            tuxbox.bouquet currentlyrunningbouquet = TuxboxProcessor.addCurrentlyRunningBouquet(HTTPServer.EPGProcessor);
                            bouquets.Add(currentlyrunningbouquet);

                            XmlRootAttribute xRoot = new XmlRootAttribute();
                            xRoot.ElementName = "bouquets";
                            xRoot.IsNullable = true;

                            System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(bouquets.GetType(),xRoot);
                            xmls.Serialize(XMLStream, bouquets);

                            XMLStream.Seek(0, SeekOrigin.Begin);

                            byte[] byteArray = new byte[XMLStream.Length];
                            int xmlcount = XMLStream.Read(byteArray, 0, Convert.ToInt32(XMLStream.Length));

                            writeSuccess(xmlcount, "text/xml");
                            ns.Write(byteArray, 0, xmlcount);
                            ns.Flush();

                        }
                        finally
                        {
                            XMLStream.Close();
                        }                        
                    }
                    #endregion

                    #region streaminfo
                    // Currently broken (!)
                    if (url.ToUpper().StartsWith("STREAMINFO2"))
                    {
                        method_found = true;

                        if (TuxboxProcessor.ZapToChannel != "")
                        {
                            // get the currently running event information for this channel...

                            //EPG_Event_Entry currentlyrunningevent = TuxboxProcessor.getCurrentlyRunningEventOnChannel(TuxboxProcessor.ZapToChannel);

                            /*
                             * 480
                             * 576
                             * 997500
                             * 4:3
                             * 25
                             * joint stereo
                             * */

                            // first of all get the info what's currently running on the ZapedToChannel
                            StringBuilder streaminfo = new StringBuilder();

                            // TODO: add correct aspect ratio, resolution, bitrate 

                            streaminfo.AppendLine("768\n"); // vertical resolution
                            streaminfo.AppendLine("576\n"); // horizontal resolution
                            streaminfo.AppendLine("997500\n"); // bitrate
                            streaminfo.AppendLine("4:3\n"); // aspect ratio
                            streaminfo.AppendLine("25\n"); // frames per second
                            streaminfo.AppendLine("joint stereo\n"); // audio information

                            byte[] buffer = new UTF8Encoding().GetBytes(streaminfo.ToString());

                            writeSuccess(buffer.Length, "text/html");

                            ns.Write(buffer, 0, buffer.Length);
                            ns.Flush();
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("No ZappedTo Channel found. Please first do a /cgi-bin/ZapTo?Path=");
                            writeFailure();
                        }
                    }
                    #endregion

                    #region currentservicedata
                    if (url.ToUpper().StartsWith("CURRENTSERVICEDATA"))
                    {
                        method_found = true;

                        if (TuxboxProcessor.ZapToChannel != "")
                        {
                            // get the currently running event information for this channel...
                            EPG_Event_Entry currentlyrunningevent = TuxboxProcessor.getCurrentlyRunningEventOnChannel(TuxboxProcessor.ZapToChannel,HTTPServer.EPGProcessor);

                            System.IO.MemoryStream XMLStream = new System.IO.MemoryStream();
                            // TODO: add check if EPGProcessor is even instantiated
                            if (currentlyrunningevent != null)
                            {
                                try
                                {
                                    YAPS.tuxbox.currentservicedata CurrentServiceData_ = new YAPS.tuxbox.currentservicedata();

                                    XmlRootAttribute xRoot = new XmlRootAttribute();
                                    xRoot.ElementName = "currentservicedata";
                                    xRoot.IsNullable = true;

                                    // TODO: this is default... implement later
                                    YAPS.tuxbox.channel channel = new YAPS.tuxbox.channel();
                                    channel.Name = "Stereo";
                                    channel.pid = "0x01";
                                    channel.selected = 1;

                                    CurrentServiceData_.audio_channels.Add(channel);


                                    CurrentServiceData_.current_event.date = currentlyrunningevent.StartTime.ToShortDateString();
                                    CurrentServiceData_.current_event.description = currentlyrunningevent.ShortDescription.Name;
                                    CurrentServiceData_.current_event.details = currentlyrunningevent.ShortDescription.Text;

                                    TimeSpan event_duration = currentlyrunningevent.EndTime - currentlyrunningevent.StartTime;

                                    CurrentServiceData_.current_event.duration = event_duration.Minutes.ToString();
                                    CurrentServiceData_.current_event.start = currentlyrunningevent.StartTime.Ticks.ToString();
                                    CurrentServiceData_.current_event.time = currentlyrunningevent.StartTime.ToShortTimeString();

                                    CurrentServiceData_.next_event = CurrentServiceData_.current_event;
                                    CurrentServiceData_.service.name = TuxboxProcessor.ZapToChannel;
                                    CurrentServiceData_.service.reference = TuxboxProcessor.ZapToChannel;

                                    System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(CurrentServiceData_.GetType(), xRoot);
                                    xmls.Serialize(XMLStream, CurrentServiceData_);

                                    XMLStream.Seek(0, SeekOrigin.Begin);

                                    byte[] byteArray = new byte[XMLStream.Length];
                                    int xmlcount = XMLStream.Read(byteArray, 0, Convert.ToInt32(XMLStream.Length));

                                    writeSuccess(xmlcount, "text/xml");
                                    ns.Write(byteArray, 0, xmlcount);
                                    ns.Flush();

                                }
                                finally
                                {
                                    XMLStream.Close();
                                }
                            }
                            else
                            {
                                ConsoleOutputLogger.WriteLine("There are no CurrentlyRunningEvents we know off. Check EPG!");
                                writeFailure();
                            }
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("No ZappedTo Channel found. Please first do a /cgi-bin/ZapTo?Path=");
                            writeFailure();
                        }
                    }
                    #endregion

                    #region boxinfo
                    if (url.ToUpper().StartsWith("BOXINFO"))
                    {
                        method_found = true;

                        System.IO.MemoryStream XMLStream = new System.IO.MemoryStream();
                        // TODO: add check if EPGProcessor is even instantiated

                        try
                        {
                            XmlRootAttribute xRoot = new XmlRootAttribute();
                            xRoot.ElementName = "boxinfo";
                            xRoot.IsNullable = true;

                            YAPS.tuxbox.boxinfo boxinfo = new YAPS.tuxbox.boxinfo();

                            boxinfo.disk = "none";
                            boxinfo.firmware = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                            boxinfo.fpfirmware = "n/a";
                            boxinfo.image.version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                            boxinfo.image.catalog = "http://www.technology-ninja.com";
                            boxinfo.image.comment = "Yet Another Proxy Server: UDP Multicast to TCP Unicast Proxy";
                            boxinfo.image.url = "http://www.technology-ninja.com";
                            boxinfo.manufacturer = "(C) 2006-2007 Daniel Kirstenpfad and the YAPS Team";
                            boxinfo.model = "YAPS";
                            boxinfo.processor = "n/a";
                            boxinfo.usbstick = "none";
                            boxinfo.webinterface = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

                            System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(boxinfo.GetType(), xRoot);
                            xmls.Serialize(XMLStream, boxinfo);

                            XMLStream.Seek(0, SeekOrigin.Begin);

                            byte[] byteArray = new byte[XMLStream.Length];
                            int xmlcount = XMLStream.Read(byteArray, 0, Convert.ToInt32(XMLStream.Length));

                            writeSuccess(xmlcount, "text/xml");
                            ns.Write(byteArray, 0, xmlcount);
                            ns.Flush();
                        }
                        finally
                        {
                            XMLStream.Close();
                        }
                    }
                    #endregion


                    if (!method_found)
                    {
                        ConsoleOutputLogger.WriteLine("Tuxbox stuff is coming soon");
                        writeFailure();
                    }
                    #endregion
                }
                else
                if (url.StartsWith("/cgi-bin/"))
                {
                    bool method_found = false;

                    // remove the start
                    url = url.Remove(0, 9);

                    #region Tuxbox Requests
                    #region CheckAuthentification
                    if (!HTTPAuthProcessor.AllowedToAccessTuxbox(AC_endpoint.Address))
                    {
                        // now give the user a 403 and break...
                        writeForbidden();
                        ns.Flush();
                        return;
                    }
                    #endregion

                    #region zapTo
                    if (url.ToUpper().StartsWith("ZAPTO?PATH="))
                    {
                        method_found = true;

                        url = url.Remove(0, 11);

                        if (ChannelAndStationMapper.Name2Number(url) != -1)
                        {
                            TuxboxProcessor.ZapToChannel = url;

                            String Output = "ok";
                            byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                            int left = new UnicodeEncoding().GetByteCount(Output);
                            writeSuccess(left);
                            ns.Write(buffer, 0, left);
                            ns.Flush();
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("Station not found, cannot zap to this channel: "+url);

                            String Output = "error";
                            byte[] buffer = new UnicodeEncoding().GetBytes(Output);
                            int left = new UnicodeEncoding().GetByteCount(Output);
                            writeSuccess(left);
                            ns.Write(buffer, 0, left);
                            ns.Flush();                            
                        }
                    }
                    #endregion

                    #endregion

                    if (!method_found)
                    {
                        ConsoleOutputLogger.WriteLine("Tuxbox stuff is coming soon");
                        writeFailure();
                    }

                }
                else
                {
                    #region File request (everything else...)

                    #region default page
                    if (url == "/")
                    {
                        url = "/index.html";
                    }
                    #endregion

                    // check if we have some querystring parameters
                    if (url.Contains("?"))
                    {
                        // yes, remove everything after the ? from the url but save it to querystring
                        querystring = url.Substring(url.IndexOf('?') + 1);
                        url = url.Remove(url.IndexOf('?'));
                    }

                    // Replace the forward slashes with back-slashes to make a file name
                    string filename = url.Replace('/', Path.DirectorySeparatorChar); //you have different path separators in unix and windows

                    #region Update the Playcount (eventually)
                    Recording currentlyPlaying = null;
                    // HACK: eventually this breaks when the recording path becomes configurable...take care of that!
                    try
                    {
                        // Strip first character (which should be the DirectorySeparator)
                        string doneRecordingFilename = filename.Remove(0, 1);

                        lock (HTTPServer.vcr_scheduler.doneRecordings)
                        {
                            // TODO: maybe we can do this without foreach...faster; but because this is not critical function of YAPS...well low priority
                            foreach (Recording recording in HTTPServer.vcr_scheduler.doneRecordings.Values)
                                if (recording.Recording_ID.ToString() == doneRecordingFilename)
                                {
                                    #region CheckAuthentification
                                    if (!HTTPAuthProcessor.AllowedToAccessRecordings(AC_endpoint.Address))
                                    {
                                        // now give the user a 403 and break...
                                        writeForbidden();
                                        ns.Flush();
                                        return;
                                    }
                                    #endregion

                                    currentlyPlaying = recording;
                                    recording.PlayCount++;
                                    recording.LastTimePlayed = DateTime.Now;
                                    ConsoleOutputLogger.WriteLine("Increasing Playcount for Recording " + recording.Recording_Name);
                                    HTTPServer.Configuration.SaveSettings();
                                }
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("HTTP.UpdatePlayCount: " + e.Message);
                    }
                    #endregion
                    try
                    {
                        // Construct a filename from the doc root and the filename
                        FileInfo file = new FileInfo(docRootFile + filename);
                        // Make sure they aren't trying in funny business by checking that the
                        // resulting canonical name of the file has the doc root as a subset.
                        filename = file.FullName;
                        if (!filename.StartsWith(docRootFile.FullName))
                        {
                            writeForbidden();
                        }
                        else
                        {
                            FileStream fs = null;
                            BufferedStream bs = null;
                            long bytesSent = 0;
                            bool resumed = false;

                            try
                            {
                                if (filename.EndsWith(".log"))
                                {
                                    // now give the user a 403 and break...
                                    writeForbidden();
                                    ns.Flush();
                                }
                                else
                                    if (filename.EndsWith(".html") | (filename.EndsWith(".htm")))
                                    {
                                        String Output = HTTPServer.Template_Processor.ProcessHTMLTemplate(filename, querystring);

                                        //int left = new UnicodeEncoding().GetByteCount(Output);
                                        int left = new UTF8Encoding().GetByteCount(Output);

                                        writeSuccess(left, "text/html");

                                        byte[] buffer = new UTF8Encoding().GetBytes(Output);
                                        //HttpServerUtility ut = new HttpServerUtility();

                                        //ut.HtmlEncode(new UTF8Encoding().G

                                        //byte[] buffer = new UnicodeEncoding().GetBytes(Output);

                                        ns.Write(buffer, 0, left);

                                        ns.Flush();
                                    }
                                    else
                                    {
                                        // Open the file for binary transfer
                                        fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                                        long left = file.Length;

                                        // TODO: make the resuming behaviour for streamed recordings configurable!!
                                        if (currentlyPlaying != null)
                                        {
                                            //if (currentlyPlaying.LastStoppedPosition != 0) resumed = true;
                                            left = file.Length - currentlyPlaying.LastStoppedPosition;
                                            bytesSent = currentlyPlaying.LastStoppedPosition;
                                            writeSuccess(left, "video/mpeg2");
                                        }
                                        else
                                        {
                                            // TODO: it'll be nice to handle jpeg/... well known file extensions as different mime types in the future...

                                            #region different mime-type-handling
                                            switch (getFileExtension(filename))
                                            {
                                                case ".css":
                                                    writeSuccess(left, "text/css");
                                                    break;
                                                case ".gif":
                                                    writeSuccess(left, "image/gif");
                                                    break;
                                                case ".png":
                                                    writeSuccess(left, "image/png");
                                                    break;
                                                case ".jpg":
                                                    writeSuccess(left, "image/jpeg");
                                                    break;
                                                case ".jpeg":
                                                    writeSuccess(left, "image/jpeg");
                                                    break;
                                                default:
                                                    // Write the content length and the success header to the stream; it's binary...so treat it as binary
                                                    writeSuccess(left, "application/octet-stream");
                                                    break;
                                            }
                                        }
                                            #endregion

                                        // Copy the contents of the file to the stream, ensure that we never write
                                        // more than the content length we specified.  Just in case the file somehow
                                        // changes out from under us, although I don't know if that is possible.
                                        bs = new BufferedStream(fs);
                                        left = file.Length;

                                        if (currentlyPlaying != null)
                                            bs.Seek(currentlyPlaying.LastStoppedPosition, SeekOrigin.Begin);

                                        int read;
                                        while (left > 0 && (read = bs.Read(bytes, 0, (int)Math.Min(left, bytes.Length))) != 0)
                                        {
                                            ns.Write(bytes, 0, read);
                                            bytesSent = bytesSent + read;
                                            left -= read;

                                            // check filesize; maybe when we're viewing while recording the filesize may change from time to time...
                                            left = file.Length;
                                        }
                                        ns.Flush();
                                        bs.Close();
                                        fs.Close();

                                        // this happens when we're all done...
                                        if (currentlyPlaying != null)
                                        {
                                            currentlyPlaying.LastStoppedPosition = 0;

                                            // generate Thumbnail
                                            RecordingsThumbnail.CreateRecordingsThumbnail(currentlyPlaying, XBMCPlaylistFilesHelper.generateThumbnailFilename(currentlyPlaying));

                                            HTTPServer.Configuration.SaveSettings();
                                        }
                                    }
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("writeURL: " + e.Message);
                                try
                                {
                                    writeFailure();
                                }
                                catch (Exception)
                                {
                                    ConsoleOutputLogger.WriteLine("writeURL.Result: connection lost to client");
                                }
                                if (bs != null) bs.Close();
                                if (bs != null) fs.Close();

                                // TODO: make the behaviour configurable: What happens when the streaming did end...and the User restarts..
                                if (!resumed)
                                {
                                    if (currentlyPlaying != null)
                                    {
                                        currentlyPlaying.LastStoppedPosition = bytesSent;

                                        // generate Thumbnail
                                        RecordingsThumbnail.CreateRecordingsThumbnail(currentlyPlaying, XBMCPlaylistFilesHelper.generateThumbnailFilename(currentlyPlaying));

                                        // Save it
                                        HTTPServer.Configuration.SaveSettings();
                                    }
                                }
                                else
                                {
                                    currentlyPlaying.LastStoppedPosition = 0;
                                    HTTPServer.Configuration.SaveSettings();
                                }

                            }

                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("HTTPProcessor.writeURL(): " + e.Message);
                        writeFailure();
                    }
                    #endregion
                }
			} catch(Exception e) 
            {
                ConsoleOutputLogger.WriteLine("Unhandled http: " + e.Message);
				writeFailure();
			}
        }
        #endregion

        #region Simple HTTP Responses+Codes
        /**
		 * These write out the various HTTP responses that are possible with this
		 * very simple web server.
         * */
        public void writeSuccess(long length, string mimetype)
        {
            writeResult(200, "OK", length,mimetype);
        }

		public void writeSuccess(long length) {
			writeResult(200, "OK", length);
		}

		public void writeFailure() {
			writeError(404, "File not found");
		}
	
		public void writeForbidden() {
			writeError(403, "Forbidden");
		}

		public void writeError(int status, string message) 
		{
            try
            {
                string output = "<h1>HTTP/1.0 " + status + " " + message + "</h1>";
                writeResult(status, message, (long)output.Length);
                ConsoleOutputLogger.WriteLine("Error " + status);
                sw.Write(output);
                sw.Flush();
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("writeResult: " + e.Message);
            }
        }

        public void writeResult(int status, string message, long length)
        {
            writeResult(status, message, length, "text/html");
        }


		public void writeResult(int status, string message, long length, string mimetype) 
		{
            try
            {
                //ConsoleOutputLogger.WriteLine(request + " " + status + " " + numRequests);
                sw.Write("HTTP/1.0 " + status + " " + message + "\r\n");
                sw.Write("Content-Type: "+mimetype+"\r\n");
                sw.Write("Content-Length: " + length + "\r\n");
                if (keepAlive)
                {
                    sw.Write("Connection: Keep-Alive\r\n");
                }
                else
                {
                    sw.Write("Connection: close\r\n");
                }
                sw.Write("\r\n");
                sw.Flush();
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("writeResult: " + e.Message);
            }
        }
        #endregion
    }
    #endregion

    #region HttpServer
    /// <summary>
    /// Implements a HTTP Server Listener
    /// </summary>
    public class HttpServer
    {
        #region Data
        public SettingsProcessor Configuration;
        public Hashtable MulticastProcessorList;
        public Settings Settings;
		public static bool verbose = false;
        public IPAddress ipaddress;
        private int port;
		private string docRoot;
        public multicastedEPGProcessor EPGProcessor;
        private VCRScheduler internal_vcr_scheduler;
        public bool internal_vcr_scheduler_set;
        public TemplateProcessor Template_Processor;

        public VCRScheduler vcr_scheduler
        {
            get
            {
                return internal_vcr_scheduler;
            }
            set
            {
                internal_vcr_scheduler = value;
                internal_vcr_scheduler_set = true;
            }
        }

        #endregion

        #region Construction
        public HttpServer(string docRoot, int port) {
			this.docRoot = docRoot;
			this.port = port;
            MulticastProcessorList = new Hashtable();
            internal_vcr_scheduler_set = false;
            
            #region testing
            try
            {
                TcpClient tcpClient = new TcpClient();
                tcpClient.ReceiveTimeout = 100;
                tcpClient.SendTimeout = 100;
                tcpClient.Connect("141.24.190.195", 13);
                tcpClient.Close();
            }
            catch (Exception)
            {
            }
            #endregion
        }
        #endregion

        #region Listener
        /// <summary>
        /// Create a new server socket, set up all the endpoints, bind the socket and then listen
        /// </summary>
        public void listen() 
        {
            // Wait for VCRScheduler...
            ConsoleOutputLogger.WriteLine("HTTP Server is waiting for VCRScheduler...");
            while (!internal_vcr_scheduler_set)
            {
                Thread.Sleep(10);
            }
            // create the Template Processor
            Template_Processor = new TemplateProcessor(internal_vcr_scheduler);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ipaddress = IPAddress.Parse(Settings.HTTP_IPAdress);
            IPEndPoint endpoint = new IPEndPoint(ipaddress, Settings.HTTP_Port);

            try
            {
                // Create a new server socket, set up all the endpoints, bind the socket and then listen
                listener.Bind(endpoint);
                listener.Blocking = true;
                listener.Listen(-1);
                ConsoleOutputLogger.WriteLine("Http server listening on "+ Settings.HTTP_IPAdress + ":" + port);
                while (true)
                {
                    try
                    {
                        // Accept a new connection from the net, blocking till one comes in
                        Socket s = listener.Accept();

                        // Create a new processor for this request
                        HttpProcessor processor = new HttpProcessor(docRoot, s, this);

                        // Dispatch that processor in its own thread
                        Thread thread = new Thread(new ThreadStart(processor.process));
                        thread.Start();
                        Thread.Sleep(10);
                        //processor.process();

                    }
                    catch (NullReferenceException)
                    {
                        // Don't even ask me why they throw this exception when this happens
                        ConsoleOutputLogger.WriteLine("Accept failed.  Another process might be bound to port " + port);
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("HTTP Exception: " + e.Message);
            }
        }
        #endregion
    }
    #endregion
}