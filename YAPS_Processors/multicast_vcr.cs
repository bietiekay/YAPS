using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace YAPS
{
    #region VCR and Streaming Request handling
    /// <summary>
    /// Handles the incoming requests and the internal Multicast Processor List and Receiver List
    /// </summary>
    public class VCRandStreaming
    {
        #region Data...
        private YAPS.HttpServer internal_HTTP_Server_Object;
        private YAPS.HttpProcessor internal_HTTP_Processor_Object;
        private YAPS.MulticastProcessor internal_Multicast_Processor_Object;
        public Guid myID;
        private StationAndChannel myChannel;
        public bool done;
        public bool internal_is_VCR;
        private byte[] cache;
        private int cache_length;
        private int cache_size;
        private FileStream recorder_stream;
        BinaryWriter binary_recorder_writer;
        public Recording internal_recording_info;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor...sets some options...
        /// </summary>
        /// <param name="isVCR"></param>
        public VCRandStreaming(bool isVCR, Recording recording_info, HttpServer iHTTP)
        {
            myID = Guid.NewGuid();
            internal_HTTP_Server_Object = iHTTP;
            done = false;
            internal_is_VCR = isVCR;
            internal_recording_info = recording_info;

            if (isVCR)
            {
                // OLD: recorder_stream = File.Create(internal_recording_info.Recording_Filename);

                recorder_stream = new FileStream(internal_recording_info.Recording_Filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                binary_recorder_writer = new BinaryWriter(recorder_stream); 
                // TODO: make the file cache size configurable
                cache_size = 400 * 1316;
                cache = new byte[cache_size];
                // TODO: sometimes the cache size can be too big so the harddisk cannot write all the data within a tolerable time, we should check that and increase/decrease the cache size accordingly
                cache_length = 0; // it's brand new!!

                // set timeout
                
                #region Create the currently-recording playlist file for the XBMC player
                using (StreamWriter sw = new StreamWriter(XBMCPlaylistFilesHelper.generateCurrentlyRecordingPlaylistFilename(internal_recording_info)))
                {
                    // Add some text to the file.
                    if (internal_HTTP_Server_Object.Settings.HTTP_Port == 80)
                        sw.Write(internal_HTTP_Server_Object.Settings.HTTP_URL + "/" + internal_recording_info.Recording_Filename);
                    else
                        sw.Write(internal_HTTP_Server_Object.Settings.HTTP_URL + ":" + internal_HTTP_Server_Object.Settings.HTTP_Port.ToString() + "/" + internal_recording_info.Recording_Filename);

                    ConsoleOutputLogger.WriteLine("Created CurrentlyRecording Playlist File...");
                }
                #endregion                
            }
        }
        #endregion

        #region Shutdown Management
        /// <summary>
        /// this is called when the thread is done...the done variable is checked by the http_processor every 100msec
        /// to see if this thread is actually done so the http_processor can terminate the connection; if it's a vcr
        /// object the vcr file is closed...
        /// </summary>
        public void youreDone()
        {
            ConsoleOutputLogger.WriteLine(internal_recording_info.Recording_Name + " is done...");
            done = true;
            internal_recording_info.CurrentlyRecording = false;
            if (internal_is_VCR)
            {
                ConsoleOutputLogger.WriteLine("Shutting down VCR filewriter stuff...");
                binary_recorder_writer.Write(cache, 0, cache_length);
                binary_recorder_writer.Close();
                recorder_stream.Close();

                // get the filesize and save it to the recording info
                FileInfo objFileSize = new FileInfo(internal_recording_info.Recording_Filename);
                internal_recording_info.FileSize = objFileSize.Length;
                ConsoleOutputLogger.WriteLine("Recording filesize is " + internal_recording_info.FileSize);

                #region Create the playlist file for the XBMC player
                // first remove the CurrentlyRecording Playlistfile

                if (XBMCPlaylistFilesHelper.removeCurrentlyRecordingPlaylistFilename(internal_recording_info))
                {
                    ConsoleOutputLogger.WriteLine("Deleted CurrentlyRecordingPlaylistFile...");
                }

                using (StreamWriter sw = new StreamWriter(XBMCPlaylistFilesHelper.generatePlaylistFilename(internal_recording_info)))
                {
                    // Add some text to the file.
                    // Add some text to the file.
                    if (internal_HTTP_Server_Object.Settings.HTTP_Port == 80)
                        sw.Write(internal_HTTP_Server_Object.Settings.HTTP_URL + "/" + internal_recording_info.Recording_Filename);
                    else
                        sw.Write(internal_HTTP_Server_Object.Settings.HTTP_URL + ":" + internal_HTTP_Server_Object.Settings.HTTP_Port.ToString() + "/" + internal_recording_info.Recording_Filename);

                    ConsoleOutputLogger.WriteLine("Created CurrentlyRecording Playlist File...");
                }

                RecordingsThumbnail.CreateRecordingsThumbnail(internal_recording_info, XBMCPlaylistFilesHelper.generateThumbnailFilename(internal_recording_info));

                #endregion
            }
            // Save settings
            internal_HTTP_Server_Object.Configuration.SaveSettings();
        }
        #endregion

        #region HandleVCR
        /// <summary>
        /// Handles a single VCR recording and spawns a new MulticastProcessor if needed or reuses one if existing
        /// </summary>
        /// <param name="url"></param>
        /// <param name="internal_http_server_object"></param>
        public void HandleVCR(StationAndChannel Channel, YAPS.HttpServer internal_http_server_object)
        {
            internal_HTTP_Server_Object = internal_http_server_object;

            #region build the channel ip endpoints...
            myChannel = Channel;
            IPAddress ip = null;
            IPEndPoint ipep;

            ip = IPAddress.Parse(myChannel.MulticastIP);
            ipep = new IPEndPoint(IPAddress.Any, int.Parse(myChannel.MulticastPort));
            #endregion

            #region MulticastProcessor checking and spawning/reusing
            if (internal_http_server_object.MulticastProcessorList.ContainsKey(myChannel.ServiceID))
            {
                // okay we have a MulticastProcessor already

                // TODO: REMOVE THIS !!!
                #region Debugging 1
                /*foreach (MulticastProcessor mpr in internal_http_server_object.MulticastProcessorList.Values)
                {
                    ConsoleOutputLogger.WriteLine(" + " + mpr.diedalready.ToString());
                    foreach (VCRandStreaming HRequest in mpr.ReceiverList.Values)
                    {
                        ConsoleOutputLogger.WriteLine("  - " + HRequest.internal_recording_info.Recording_Name);
                    }
                }*/
                #endregion

                // get that MulticastProcessor object
                internal_Multicast_Processor_Object = (MulticastProcessor)internal_http_server_object.MulticastProcessorList[myChannel.ServiceID];

                lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                {
                    // add myself to the list
                    internal_Multicast_Processor_Object.ReceiverList.Add(myID, this);
                }
            }
            else
            {
                // we don't have a MulticastProcessor yet

                // create one
                internal_Multicast_Processor_Object = new MulticastProcessor(ip, ipep, internal_http_server_object, myChannel.ServiceID.ToString(),myChannel.isRTP);
                // add him to the global list
                lock (internal_http_server_object.MulticastProcessorList.SyncRoot)
                {
                    internal_http_server_object.MulticastProcessorList.Add(myChannel.ServiceID, internal_Multicast_Processor_Object);
                }
                lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                {
                    internal_Multicast_Processor_Object.ReceiverList.Add(myID, this);
                }

                Thread thread = new Thread(new ThreadStart(internal_Multicast_Processor_Object.Go));
                thread.Start();
            }
            #endregion
        }
        #endregion

        #region HandleStreaming
        public void HandleStreaming(String url, YAPS.HttpProcessor http_processor_object)
        {
            #region build the channel ip endpoints...

            try
            {
                // try to resolve the name
                myChannel = ChannelAndStationMapper.Name2Data(url.Remove(0, 5));

                if (myChannel == null)
                {
                    myChannel = ChannelAndStationMapper.Number2Data(Convert.ToInt32(url.Remove(0, 5)));

                    // nothing to do here...
                    if (myChannel == null)
                        return;
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("HandleStreaming: " + e.Message);
            }

            IPAddress ip = null;
            IPEndPoint ipep;

            internal_HTTP_Processor_Object = http_processor_object;

            internal_HTTP_Processor_Object.ns.WriteTimeout = 1000;

            ip = IPAddress.Parse(myChannel.MulticastIP);
            ipep = new IPEndPoint(IPAddress.Any, int.Parse(myChannel.MulticastPort));
            //                    long left = file.Length;
            internal_HTTP_Processor_Object.writeSuccess(0);
            #endregion

            #region MulticastProcessor checking and spawning/reusing
            if (internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList.ContainsKey(myChannel.ServiceID))
            {
                // okay we have a MulticastProcessor already
                ConsoleOutputLogger.WriteLine("Reusing MulticastProcessor for channel " + myChannel.ChannelName);
                // get that MulticastProcessor object
                internal_Multicast_Processor_Object = (MulticastProcessor)internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList[myChannel.ServiceID];

                lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                {
                    // add myself to the list
                    internal_Multicast_Processor_Object.ReceiverList.Add(myID, this);
                }
            }
            else
            {
                // we don't have a MulticastProcessor yet
                ConsoleOutputLogger.WriteLine("Creating a new MulticastProcessor for channel " + myChannel.ChannelName);
                // create one
                internal_Multicast_Processor_Object = new MulticastProcessor(ip, ipep, internal_HTTP_Processor_Object.HTTPServer, myChannel.ServiceID.ToString(),myChannel.isRTP);
                lock (internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList.SyncRoot)
                {
                    // add him to the global list
                    internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList.Add(myChannel.ServiceID, internal_Multicast_Processor_Object);
                }

                lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                {
                    internal_Multicast_Processor_Object.ReceiverList.Add(myID, this);
                }
                Thread thread = new Thread(new ThreadStart(internal_Multicast_Processor_Object.Go));
                thread.Start();
            }
            #endregion
        }
        #endregion

        #region Data Handling
        /// <summary>
        /// all the data we're receiving gets handled here - everytime the multicast processor receives a packet
        /// this method is called - so it has to decide wether to write the data into a file or to send it out to
        /// the http client.
        /// </summary>
        /// <param name="Data">well you know, the byte array with the data (head stripped rtp)</param>
        /// <param name="length">the length of the data packet</param>
        public void SendDataToClient(byte[] Data, int length)
        {
            try
            {
                // check if we're a VCR or a Streamer
                if (!internal_is_VCR)
                {
                    #region Stream it baby...
                    // we're a streamer...just pass the data to the client...

                    // check for a timeout and then initiate the timeshifting...
                    #region TimeShifting
                    // this is how timeshift works in yaps (for now)
                    //  1. detect that the user paused the streaming
                    //  2. start to put data into the TimeShiftProcessor
                    //  3. when user unpauses retrieve the data from there (keep it in this thread and let the multicast_vcr Processor put data into the TimeShiftProcessor from other threads)
                    #endregion

                    try
                    {
                        internal_HTTP_Processor_Object.ns.Write(Data, 0, length);
                    }
                    catch (SocketException sex)
                    {
                        ConsoleOutputLogger.WriteLine("Socket Timeout while sending: " + sex.Message);
                    }
                    #endregion
                }
                else
                {
                    #region Record it baby...
                    // we're a recorder, but first we check if we're done with this recording
                    if (internal_recording_info.EndsAt.Ticks >= DateTime.Now.Ticks)
                    {
                        // we want to cache some data before writing it to the file...
                        if ((cache_length + length) > (cache_size))
                        {
                            //ConsoleOutputLogger.WriteLine("Cachesize: " + length + " Cachefill: " + cache_length);
                            // write the cache to the drive
                            
                            //binary_recorder_writer.Write(cache, 0, cache_length);

                            CachedThreadedWriter CacheWriter = new CachedThreadedWriter(binary_recorder_writer, cache, cache_length);

                            Thread writer_thread = new Thread(new ThreadStart(CacheWriter.WriterThread));
                            writer_thread.Start();


                            // empty the cache and get everything up-n-running for the next cache-filling round...
                            cache_length = 0;
                            Array.ConstrainedCopy(Data, 0, cache, cache_length, length);
                            cache_length = cache_length + length;

                        }
                        else
                        {
                            // add to the cache...
                            Array.ConstrainedCopy(Data, 0, cache, cache_length, length);
                            cache_length = cache_length + length;
                        }
                    }
                    else
                    {
                        // set the status bit that we're currently not recording anymore...
                        internal_recording_info.CurrentlyRecording = false;

                        // Save the new Configuration...
                        //internal_HTTP_Server_Object.Configuration.SaveSettings();

                        // we're done and so is this vcr
                        youreDone();

                        lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                        {
                            // check if there's no one left to watch or record
                            if (internal_Multicast_Processor_Object.ReceiverList.Count == 1)
                            {
                                lock (internal_HTTP_Server_Object.MulticastProcessorList.SyncRoot)
                                {
                                    // remove the MulticastProcessor Object from the global list
                                    internal_HTTP_Server_Object.MulticastProcessorList.Remove(myChannel.ServiceID);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                #region If anything went wrong, we will end up here...
                ConsoleOutputLogger.WriteLine("SendDataToClient: "+e.Message);
                try
                {
                    youreDone();
                }
                catch(Exception e2)
                {
                    ConsoleOutputLogger.WriteLine("Cache flushing error: "+e2.Message);
                }

                lock (internal_Multicast_Processor_Object.ReceiverList.SyncRoot)
                {
                    // check if there's no one left to watch or record
                    if (internal_Multicast_Processor_Object.ReceiverList.Count == 1)
                    {
                        // remove the MulticastProcessor Object from the global list
                        lock (internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList.SyncRoot)
                        {
                            internal_HTTP_Processor_Object.HTTPServer.MulticastProcessorList.Remove(myChannel.ServiceID);
                        }
                    }
                }
                // set us to done and close everything
                done = true;
                #endregion
            }
        }
        #endregion
    }
    #endregion

    #region Cached and Threaded Writer
    public class CachedThreadedWriter
    {
        private int CacheSize;
        private byte[] Cache;
        private BinaryWriter internal_Writer;

        public CachedThreadedWriter(BinaryWriter Writer, byte[] CacheToWrite, int BytesToWrite)
        {
            internal_Writer = Writer;
            Cache = new byte[BytesToWrite];
            Array.ConstrainedCopy(CacheToWrite, 0, Cache, 0, BytesToWrite);
            CacheSize = BytesToWrite;
        }

        public void WriterThread()
        {
            lock(internal_Writer)
            {
                internal_Writer.Write(Cache, 0, CacheSize);
            }
        }
    }
    #endregion

    #region Multicast and VCR
    public class MulticastProcessor
    {
        public Hashtable ReceiverList;
        private IPEndPoint ipep;
        private IPAddress ip;
        private YAPS.HttpServer internal_http_server_object;
        private string my_ID;
        public bool diedalready;
        private bool isRTP;

        public MulticastProcessor(IPAddress ip_address, IPEndPoint ip_endpoint, YAPS.HttpServer http_server_object, string MulticastProcessorID, bool isRTP_)
        {
            ReceiverList = new Hashtable();
            ipep = ip_endpoint;
            ip = ip_address;
            my_ID = MulticastProcessorID;
            internal_http_server_object = http_server_object;
            diedalready = false;
            isRTP = isRTP_;
        }

        public void Go()
        {
            ConsoleOutputLogger.WriteLine("Multicast Processor adds Membership to " + ip.ToString());
        	// Add us to the Multicast Group
        	#if MONO
        		MonoSocket s = new MonoSocket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        	#else
        		Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        	#endif
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            s.Bind(ipep);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

            // allocate the buffer
            byte[] b = new byte[1400];
            s.ReceiveTimeout = 5000;   // 5 second timeout
            int retrycount = 0;
            bool served_well; // is false when no client was served this iteration...so we can die...

            // the main loop to receive the data and send it out to all the clients
            while ((true) && (retrycount <= 2))
            {
                try
                {
                    // receive it!
                    int blength = s.Receive(b);

                    // if this channel uses RTP - strip the RTP headers...
                    if (isRTP)
                    {
                        byte[] ob = rtp.killRTPheader(b, ref blength);
                        b = ob;
                    }

                    // check if no one wants to see us...
                    if (ReceiverList.Count == 0)
                    {
                        ConsoleOutputLogger.WriteLine("No One wants to see us...so we're going to die now...");
                        // fail-safe: remove us from the list
                        lock (internal_http_server_object.MulticastProcessorList.SyncRoot)
                        {
                            internal_http_server_object.MulticastProcessorList.Remove(my_ID);
                        }
                        break;
                    }

                    served_well = false;
                    lock (ReceiverList.SyncRoot)
                    {
                        foreach (VCRandStreaming HRequest in ReceiverList.Values)
                        {
                            if (!HRequest.done)
                            {
                                HRequest.SendDataToClient(b, blength);
                                served_well = true;
                            }
                            else
                            {
                                // do nothing with this object, just ignore it this time...our top priority is to
                                // get the bytes out to all clients..

                                ////ConsoleOutputLogger.WriteLine("One VCR Client is done, so we remove him from the Receiverlist...");
                                ////ReceiverList.Remove(HRequest.myID
                            }
                        }
                    }
                    retrycount = 0;
                    
                    // die if we did not serve anybody this iteration
                    if (!served_well) break;
                }
                catch (Exception e)
                {
                    ConsoleOutputLogger.WriteLine("Go Exception: " + e.Message);
                    retrycount++;
                }
            }

            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, new MulticastOption(ip, IPAddress.Any));
            s.Shutdown(SocketShutdown.Both);
            s.Close();

            // tell everybody that we're about to die...
            try
            {
                ConsoleOutputLogger.WriteLine("Telling every Client that this Channel blacked out...");
                foreach (VCRandStreaming HRequest in ReceiverList.Values)
                {
                    if (!HRequest.done)
                    {
                        HRequest.youreDone();
                        ConsoleOutputLogger.WriteLine("Telling Recording " + HRequest.internal_recording_info.Recording_Name + " that we blacked out..");
                    }
                }
                // shutting down...
                ReceiverList.Clear();
                // Removing MulticastProcessor from the MulticastProcessorList...
                lock (internal_http_server_object.MulticastProcessorList.SyncRoot)
                {
                    internal_http_server_object.MulticastProcessorList.Remove(my_ID);
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("Dying Exception: " + e.Message);
            }
            ConsoleOutputLogger.WriteLine("MulticastProcessor for channel "+ip.ToString()+" is done...");
            diedalready = true;
        }
    }
    #endregion
}
