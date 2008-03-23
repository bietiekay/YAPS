using System;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace YAPS
{
    /// <summary>
    /// each file request that leads to a .html file is passed through here
    /// </summary>
    public class TemplateProcessor
    {
        #region internal Data
        private VCRScheduler internal_vcrscheduler;
        //private HttpProcessor internal_httpprocessor;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor of the Templace Processor
        /// </summary>
        /// <param name="vcrscheduler">the VCRScheduler...so we can get the </param>
        public TemplateProcessor(VCRScheduler vcrscheduler)
        {
            internal_vcrscheduler = vcrscheduler;
        }
        #endregion

        #region ProcessHTMLTemplate
        /// <summary>
        /// Processes the actual HTML Sourcecode
        /// </summary>
        /// <param name="template_filename"></param>
        /// <returns>the actual HTML code after the template parsing and keyword replaceing</returns>
        public String ProcessHTMLTemplate(String template_filename,String Querystring, String Username)
        {
            String Output_HTML_Code = "";
            DateTime Started = DateTime.Now;

            #region read the template file
            using (StreamReader sr = File.OpenText(template_filename))
            {
                String buffer;

                while ((buffer = sr.ReadLine()) != null)
                {
                    Output_HTML_Code = Output_HTML_Code + buffer;
                    // read...
                }
                sr.Close();
            }
            #endregion

            #region replace the placeholders...

            // TODO: at the moment the placeholders must be lower case only to be detected - change that!
            // TODO: 3 times the same keyword... change that

            #region no need for querystring or optional

                #region %include($templateURL)%
                // first find the placeholder

                // detect if we included a file again in the last 10 iterations
                List<string> LoopDetection = new List<string>();


                while (Output_HTML_Code.Contains("%include("))
                {

                    // TODO: make a URL version to include http URLs as well
                    // now this is the first placeholder that has a parameter, the next step would be to extract the parameter...
                    try
                    {
                        #region Find and extract the parameter, then delete it with %include( in front of and ) behind
                        int StartPosition = Output_HTML_Code.IndexOf("%include(");

                        // add the parameter...
                        StartPosition = StartPosition + 9;

                        // we need a working copy...it's easier...
                        String parameters = Output_HTML_Code.Remove(0, StartPosition);

                        // let's find the next ) and remove everything, including the ) afterwards
                        StartPosition = parameters.IndexOf(')');
                        // we got them!!!
                        parameters = parameters.Remove(StartPosition);

                        StartPosition = Output_HTML_Code.IndexOf("%include(");
                        // delete them from the original HTML_Code
                        Output_HTML_Code = Output_HTML_Code.Remove(Output_HTML_Code.IndexOf("%include("), 11 + parameters.Length);
                        Output_HTML_Code = Output_HTML_Code.Insert(StartPosition, "%include_template%");
                        #endregion

                        String newTemplate = "";

                        if (LoopDetection.Contains(parameters))
                        {
                            ConsoleOutputLogger.WriteLine("%include%-Parser: possible loop found for " + parameters);
                        }
                        else
                        {

                            // this is just to make sure that this could not be used as a DoS attack vector
                            if (LoopDetection.Count == 500)
                            {
                                LoopDetection.RemoveAt(0);
                            }

                            LoopDetection.Add(parameters);

                            #region read the to be included template file
                            try
                            {
                                using (StreamReader sr = File.OpenText(parameters))
                                {
                                    String buffer;

                                    while ((buffer = sr.ReadLine()) != null)
                                    {
                                        newTemplate = newTemplate + buffer;
                                        // read...
                                    }
                                    sr.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("%include%-Parser: " + e.Message);
                                // delete the include_template...
                                Output_HTML_Code = Output_HTML_Code.Replace("%include_template%", "");
                            }
                            #endregion

                            Output_HTML_Code = Output_HTML_Code.Replace("%include_template%", newTemplate);
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("%include%-Parser: " + e.Message);
                    }
                }
                #endregion

                #region %querystring%
                // first find the placeholder
                while (Output_HTML_Code.Contains("%querystring%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%querystring%", Querystring);
                }
                #endregion

                #region %rendertransformation($input_url,$xslt_url)%
                // TODO: implement
                
                #endregion

                #region %render_recorded_table($line_templateURL)%
                // first find the placeholder

                // detect if we included a file again in the last 10 iterations
                while (Output_HTML_Code.Contains("%render_recorded_table("))
                {

                    // TODO: make a URL version to include http URLs as well
                    // now this is the first placeholder that has a parameter, the next step would be to extract the parameter...
                    try
                    {
                        #region Find and extract the parameter, then delete it with %include( in front of and ) behind
                        int StartPosition = Output_HTML_Code.IndexOf("%render_recorded_table(");

                        // add the parameter...
                        StartPosition = StartPosition + 23;

                        // we need a working copy...it's easier...
                        String parameters = Output_HTML_Code.Remove(0, StartPosition);

                        // let's find the next ) and remove everything, including the ) afterwards
                        StartPosition = parameters.IndexOf(')');
                        // we got them!!!
                        parameters = parameters.Remove(StartPosition);

                        StartPosition = Output_HTML_Code.IndexOf("%render_recorded_table(");
                        // delete them from the original HTML_Code
                        Output_HTML_Code = Output_HTML_Code.Remove(Output_HTML_Code.IndexOf("%render_recorded_table("), 25 + parameters.Length);
                        Output_HTML_Code = Output_HTML_Code.Insert(StartPosition, "%render_recorded_table_template%");
                        #endregion

                        String newTemplate = "";

                        if (LoopDetection.Contains(parameters))
                        {
                            ConsoleOutputLogger.WriteLine("%render_recorded_table%-Parser: possible loop found for " + parameters);
                        }
                        else
                        {

                            // this is just to make sure that this could not be used as a DoS attack vector
                            if (LoopDetection.Count == 500)
                            {
                                LoopDetection.RemoveAt(0);
                            }

                            LoopDetection.Add(parameters);

                            #region read the to be included template file
                            try
                            {
                                using (StreamReader sr = File.OpenText(parameters))
                                {
                                    String buffer;

                                    while ((buffer = sr.ReadLine()) != null)
                                    {
                                        newTemplate = newTemplate + buffer;
                                        // read...
                                    }
                                    sr.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("%render_recorded_table%-Parser: " + e.Message);
                                // delete the include_template...
                                Output_HTML_Code = Output_HTML_Code.Replace("%render_recorded_table_template%", "");
                            }
                            #endregion

                            // now pass the template code to the http generator...

                            // Default values for recorded_listing
                            Category FilterCategory = null;
                            bool SortAscending = false;

                            // TODO: because of the idiotic Querystring handling we cannot detect more than one parameter....fix that!

                            #region Querystring checking if we should do something more than just listing
                            try
                            {
                                string[] splitted = Querystring.Split('=');
                                // TODO: do the querystring bullshit in a specialised class...this for...stuff sucks

                                for (int i = 0; i < splitted.Length; i++)
                                {
                                    #region Category Filter
                                    if (splitted[i].ToUpper() == "CATEGORYFILTER")
                                    {
                                        // splitted[i+1] contains the category name...
                                        if (internal_vcrscheduler.Category_Processor.CategoryExists(HttpUtility.UrlDecode(splitted[i + 1])))
                                        {
                                            // hurray, the category even exists...
                                            FilterCategory = internal_vcrscheduler.Category_Processor.GetCategory(HttpUtility.UrlDecode(splitted[i + 1]));
                                        }
                                    }
                                    #endregion

                                    #region SortDescending
                                    if (splitted[i].ToUpper() == "SORTDESCENDING")
                                    {
                                        SortAscending = false;
                                    }
                                    #endregion

                                    #region SortAscending
                                    if (splitted[i].ToUpper() == "SORTASCENDING")
                                    {
                                        SortAscending = true;
                                    }
                                    #endregion
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("%recorded_listing%: " + e.Message);
                            }
                            #endregion

                            Output_HTML_Code = Output_HTML_Code.Replace("%render_recorded_table_template%", Template_Recorded_Listing(FilterCategory, SortAscending, newTemplate, Username));

                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("%render_recorded_table%-Parser: " + e.Message);
                    }
                }
                #endregion

                #region %render_recording_table($line_templateURL)%
                // first find the placeholder

                // detect if we included a file again in the last 10 iterations
                while (Output_HTML_Code.Contains("%render_recording_table("))
                {

                    // TODO: make a URL version to include http URLs as well
                    // now this is the first placeholder that has a parameter, the next step would be to extract the parameter...
                    try
                    {
                        #region Find and extract the parameter, then delete it with %include( in front of and ) behind
                        int StartPosition = Output_HTML_Code.IndexOf("%render_recording_table(");

                        // add the parameter...
                        StartPosition = StartPosition + 24;

                        // we need a working copy...it's easier...
                        String parameters = Output_HTML_Code.Remove(0, StartPosition);

                        // let's find the next ) and remove everything, including the ) afterwards
                        StartPosition = parameters.IndexOf(')');
                        // we got them!!!
                        parameters = parameters.Remove(StartPosition);

                        StartPosition = Output_HTML_Code.IndexOf("%render_recording_table(");
                        // delete them from the original HTML_Code
                        Output_HTML_Code = Output_HTML_Code.Remove(Output_HTML_Code.IndexOf("%render_recording_table("), 26 + parameters.Length);
                        Output_HTML_Code = Output_HTML_Code.Insert(StartPosition, "%render_recording_table_template%");
                        #endregion

                        String newTemplate = "";

                        if (LoopDetection.Contains(parameters))
                        {
                            ConsoleOutputLogger.WriteLine("%render_recording_table%-Parser: possible loop found for " + parameters);
                        }
                        else
                        {

                            // this is just to make sure that this could not be used as a DoS attack vector
                            if (LoopDetection.Count == 500)
                            {
                                LoopDetection.RemoveAt(0);
                            }

                            LoopDetection.Add(parameters);

                            #region read the to be included template file
                            try
                            {
                                using (StreamReader sr = File.OpenText(parameters))
                                {
                                    String buffer;

                                    while ((buffer = sr.ReadLine()) != null)
                                    {
                                        newTemplate = newTemplate + buffer;
                                        // read...
                                    }
                                    sr.Close();
                                }
                            }
                            catch (Exception e)
                            {
                                ConsoleOutputLogger.WriteLine("%render_recording_table%-Parser: " + e.Message);
                                // delete the include_template...
                                Output_HTML_Code = Output_HTML_Code.Replace("%render_recording_table_template%", "");
                            }
                            #endregion

                            // now pass the template code to the http generator...

                            Output_HTML_Code = Output_HTML_Code.Replace("%render_recording_table_template%", Template_Recording_Listing(newTemplate, Username));
                        }
                    }
                    catch (Exception e)
                    {
                        ConsoleOutputLogger.WriteLine("%render_recording_table%-Parser: " + e.Message);
                    }
                }
                #endregion

                #region %console_output%
                // first find the placeholder
                while (Output_HTML_Code.Contains("%console_output%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%console_output%", Template_Console_Output());
                }
                #endregion

                #region %space_usage_bar%
                // first find the placeholder
                while (Output_HTML_Code.Contains("%space_usage_bar%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%space_usage_bar%", Template_Space_Usage_Bar());
                }
                #endregion

                #region %page_render_time%
                DateTime ended = DateTime.Now;
                while (Output_HTML_Code.Contains("%page_render_time%"))
                {
                    TimeSpan render_time = new TimeSpan(ended.Ticks-Started.Ticks);

                    Output_HTML_Code = Output_HTML_Code.Replace("%page_render_time%", Convert.ToString(render_time.TotalMilliseconds)+"msecs");
                }

                #endregion

                #region %http_url%
                while (Output_HTML_Code.Contains("%http_url%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%http_url%", internal_vcrscheduler.Settings.HTTP_URL);
                }
                #endregion

                #region %skin%
                while (Output_HTML_Code.Contains("%skin%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%skin%", internal_vcrscheduler.Settings.Skin);
                }
                #endregion

                #region %all_categories_listing%
                // this outputs <option> code with all categories...
                while (Output_HTML_Code.Contains("%all_categories_listing%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%all_categories_listing%", Template_All_Categories_Listing());
                }
                #endregion

                #region %build_version%
                while (Output_HTML_Code.Contains("%build_version%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%build_version%", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }

                #endregion

                #region Settings

                #region %settings_minimum_free_filesystem_space%
                while (Output_HTML_Code.Contains("%settings_minimum_free_filesystem_space%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_minimum_free_filesystem_space%", Convert.ToString(internal_vcrscheduler.Settings.Minimum_Free_Filesystem_Space));
                }
                #endregion

                #region %settings_vcr_root_directory%
                while (Output_HTML_Code.Contains("%settings_vcr_root_directory%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_vcr_root_directory%", internal_vcrscheduler.Settings.VCR_Root_Directory);
                }
                #endregion

                #region %settings_restart_stream_after_disconnect%
                while (Output_HTML_Code.Contains("%settings_restart_stream_after_disconnect%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_restart_stream_after_disconnect%", internal_vcrscheduler.Settings.rewind_after_disconnect.ToString());                }
                #endregion

                #region %settings_multicast_cached_and_threaded_writer_buffer_size%
                while (Output_HTML_Code.Contains("%settings_multicast_cached_and_threaded_writer_buffer_size%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_multicast_cached_and_threaded_writer_buffer_size%", Convert.ToString(internal_vcrscheduler.Settings.Multicast_Cached_and_Threaded_Writer_Buffer_Size));
                }
                #endregion

                #region %settings_maximum_number_of_concurrent_clients%
                while (Output_HTML_Code.Contains("%settings_maximum_number_of_concurrent_clients%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_maximum_number_of_concurrent_clients%", Convert.ToString(internal_vcrscheduler.Settings.Maximum_Number_Of_Concurrent_Clients));
                }
                #endregion

                #region %settings_http_root_directory%
                while (Output_HTML_Code.Contains("%settings_http_root_directory%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_http_root_directory%", internal_vcrscheduler.Settings.HTTP_Root_Directory);
                }
                #endregion

                #region %settings_http_url%
                while (Output_HTML_Code.Contains("%settings_http_url%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_http_url%", internal_vcrscheduler.Settings.HTTP_URL);
                }
                #endregion

                #region %settings_http_buffer_size%
                while (Output_HTML_Code.Contains("%settings_http_buffer_size%"))
                {
                    Output_HTML_Code = Output_HTML_Code.Replace("%settings_http_buffer_size%", Convert.ToString(internal_vcrscheduler.Settings.HTTP_Buffer_Size));
                }
                #endregion
                #endregion

            #endregion

                #region needs querystring
                if (Querystring != "")
                {
                    #region %category%
                    // this scans the querystring for the category name and replaces it in the html code
                    while (Output_HTML_Code.Contains("%category%"))
                    {
                        try
                        {
                            // we don't check anything here; just get the code out fast...
                            string[] splitted = Querystring.Split('=');

                            for (int i = 0; i < splitted.Length; i++)
                            {
                                if (splitted[i].ToUpper() == "CATEGORY")
                                {
                                    Output_HTML_Code = Output_HTML_Code.Replace("%category%", splitted[i + 1]);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleOutputLogger.WriteLine("TemplateProcessor.Category: " + e.Message);
                            Output_HTML_Code = Output_HTML_Code.Replace("%category%", "");
                        }
                    }
                    #endregion

                    #region %categoryname%
                    // this scans the querystring for the category name, finds the object in the CategoryList and returns the name
                    while (Output_HTML_Code.Contains("%categoryname%"))
                    {
                        try
                        {
                            string[] splitted = Querystring.Split('=');
                            // TODO: do the querystring bullshit in a specialised class...this for...stuff sucks
                            for (int i = 0; i < splitted.Length; i++)
                            {
                                if (splitted[i].ToUpper() == "CATEGORY")
                                {
                                    // splitted[i+1] contains the category name...
                                    if (internal_vcrscheduler.Category_Processor.CategoryExists(HttpUtility.UrlDecode(splitted[i+1], Encoding.UTF7)))
                                    {
                                        // hurray, the category even exists...
                                        Category tempCategory = internal_vcrscheduler.Category_Processor.GetCategory(HttpUtility.UrlDecode(splitted[i + 1], Encoding.UTF7));
                                        Output_HTML_Code = Output_HTML_Code.Replace("%categoryname%", tempCategory.Name);
                                    }
                                    else
                                        Output_HTML_Code = Output_HTML_Code.Replace("%categoryname%", "");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleOutputLogger.WriteLine("TemplateProcessor.CategoryName: " + e.Message);
                            Output_HTML_Code = Output_HTML_Code.Replace("%categoryname%", "");
                        }
                    }
                    #endregion

                    #region %categorydescription%
                    // this scans the querystring for the category name, finds the object in the CategoryList and returns the comment
                    while (Output_HTML_Code.Contains("%categorydescription%"))
                    {
                        try
                        {
                            string[] splitted = Querystring.Split('=');
                            // TODO: do the querystring bullshit in a specialised class...this for...stuff sucks

                            for (int i = 0; i < splitted.Length; i++)
                            {
                                if (splitted[i].ToUpper() == "CATEGORY")
                                {
                                    // splitted[i+1] contains the category name...
                                    if (internal_vcrscheduler.Category_Processor.CategoryExists(HttpUtility.UrlDecode(splitted[i + 1], Encoding.UTF7)))
                                    {
                                        // hurray, the category even exists...
                                        Category tempCategory = internal_vcrscheduler.Category_Processor.GetCategory(HttpUtility.UrlDecode(splitted[i + 1], Encoding.UTF7));
                                        Output_HTML_Code = Output_HTML_Code.Replace("%categorydescription%", tempCategory.Comment);
                                    }
                                    else
                                        Output_HTML_Code = Output_HTML_Code.Replace("%categorydescription%", "");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleOutputLogger.WriteLine("TemplateProcessor.categorydescription: " + e.Message);
                            Output_HTML_Code = Output_HTML_Code.Replace("%categorydescription%", "");
                        }
                    }
                    #endregion

                    #region %all_category_searchterms%
                    // this scans the querystring for the category name, finds the object in the CategoryList and returns the searchterms
                    while (Output_HTML_Code.Contains("%all_category_searchterms%"))
                    {
                        try
                        {
                            string[] splitted = Querystring.Split('=');
                            // TODO: do the querystring bullshit in a specialised class...this for...stuff sucks

                            for (int i = 0; i < splitted.Length; i++)
                            {
                                if (splitted[i].ToUpper() == "CATEGORY")
                                {
                                    // splitted[i+1] contains the category name...
                                    if (internal_vcrscheduler.Category_Processor.CategoryExists(HttpUtility.UrlDecode(splitted[i + 1], Encoding.UTF7)))
                                    {
                                        // hurray, the category even exists...
                                        Category tempCategory = internal_vcrscheduler.Category_Processor.GetCategory(HttpUtility.UrlDecode(splitted[i + 1], Encoding.UTF7));
                                        Output_HTML_Code = Output_HTML_Code.Replace("%all_category_searchterms%", Template_All_Category_Searchterms(tempCategory));
                                    }
                                    else
                                        Output_HTML_Code = Output_HTML_Code.Replace("%all_category_searchterms%", "");
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            ConsoleOutputLogger.WriteLine("TemplateProcessor.all_category_searchterms: " + e.Message);
                            Output_HTML_Code = Output_HTML_Code.Replace("%call_category_searchterms%", "");
                        }
                    }

                    #endregion

                }
            #endregion

            #endregion

            return Output_HTML_Code;
        }
        #endregion

        #region HTML Code Generators

        #region Space_Usage_Bar
        /// <summary>
        /// Draws a Percentage Bar of the ration of free and used space
        /// </summary>
        /// <returns>HTML Code</returns>
        String Template_Space_Usage_Bar()
        {
        	StringBuilder Output = new StringBuilder();
        	long FreeSpace = 1;
        	long TotalSize = 1;
        	
        	#if MONO
        	int longestMatch = 0;
        	Mono.Unix.UnixDriveInfo[] mountPoints = Mono.Unix.UnixDriveInfo.GetDrives();
        	foreach(Mono.Unix.UnixDriveInfo mountPoint in mountPoints) //we get only the mountpoints, so search for the mountpoint, which contains this directory
        	{
        		//search for the mount point which has the longest match (do not take "/" as mountpoint for "/mnt/data/Video" but "/mnt/data")
        		if (Environment.CurrentDirectory.StartsWith(mountPoint.RootDirectory.FullName) && longestMatch < mountPoint.RootDirectory.FullName.Length)
        		{
        			FreeSpace = mountPoint.AvailableFreeSpace;
        			TotalSize = mountPoint.TotalSize;
        			longestMatch = mountPoint.RootDirectory.FullName.Length;
        		}
        	}
        	#else
        	  // TODO: configurable recording storeing path...
            System.IO.DriveInfo DiskInfo = new DriveInfo(Environment.CurrentDirectory);

            FreeSpace = DiskInfo.AvailableFreeSpace;
            TotalSize = DiskInfo.TotalSize;
        	
        	#endif
            float fFreePercent = FreeSpace/((TotalSize)/100);
            
            int FreePercent = Convert.ToInt32(fFreePercent);
            int UsedPercent = 100-FreePercent;


            // HACK: since I don't want to get every filesize at this moment I am estimating on base of the used space
            #region the blue bar
            // Calculate how much minutes used all our recordings are
            TimeSpan RecordedTime = new TimeSpan(0);
            lock (internal_vcrscheduler.doneRecordings)
            {
                foreach (Recording recording_entry in internal_vcrscheduler.doneRecordings.Values)
                {
                    if (!recording_entry.CurrentlyRecording)
                        RecordedTime = RecordedTime.Add(new TimeSpan((recording_entry.EndsAt.Ticks - recording_entry.StartsAt.Ticks)));
                }
            }
            
            // Calculate how much minutes will be used by the scheduled recordings            
            TimeSpan ToBeRecordedTime = new TimeSpan(0);

            lock (internal_vcrscheduler.Recordings)
            {
                foreach (Recording recording_entry in internal_vcrscheduler.Recordings.Values)
                {
                        ToBeRecordedTime = ToBeRecordedTime.Add(new TimeSpan((recording_entry.EndsAt.Ticks - recording_entry.StartsAt.Ticks)));
                }
            }

            // we estimate now how many percent of the free space would be used after all scheduled recordings are done
            int AvgBitrate = 0;
            // calculate the average Bitrate per Second
            if (RecordedTime.TotalSeconds != 0)
                AvgBitrate = Convert.ToInt32((TotalSize - FreeSpace) / RecordedTime.TotalSeconds);

            
            // calculate the needed Space for all our recordings
            double EstimatedNeededSpace = (AvgBitrate * ToBeRecordedTime.TotalSeconds);

            // how much percent of the free space is this?
            int EstimatedNeededSpacePercent = Convert.ToInt32((EstimatedNeededSpace / ((FreeSpace) / 100)));

            if (EstimatedNeededSpacePercent >= FreePercent)
            {
                // when more space is needed than available, draw the normal bar with no green part and the exclamation mark
                Output.Append("<table><tr>");
                Output.Append("<td id=\"space-usage-cell-used\" style=\"width: ");
                Output.Append(UsedPercent.ToString());
                Output.Append("%;\"></td>");
                //Output.Append(UsedPercent.ToString() + "%</td>");
                Output.Append("<td id=\"space-usage-cell-needed\" style=\"width: ");
                Output.Append(FreePercent.ToString());
                Output.Append("%;\"></td>");
                Output.Append("<td style=\"height:25px; text-align:center;\"><img src=\"images/exclamationmark.png\" border=\"0\" alt=\"Zuwenig Speicherplatz für alle Aufnahmen\" /></td>");
                Output.Append("</tr></table>");
            }
            else
            {
                // draw red,blue,green
                FreePercent = FreePercent - EstimatedNeededSpacePercent;

                Output.Append("<table><tr>");
                Output.Append("<td id=\"space-usage-cell-used\" style=\"width: ");
                Output.Append(UsedPercent.ToString());
                Output.Append("%; \"></td>");
                Output.Append("<td id=\"space-usage-cell-needed\" style=\"width: ");
                Output.Append(EstimatedNeededSpacePercent.ToString());
                Output.Append("%;\"></td>");
                Output.Append("<td id=\"space-usage-cell-free\" style=\"width: ");
                Output.Append(FreePercent.ToString());
                Output.Append("%; \"></td>");                
                Output.Append("</tr></table>");
            }
            

            #endregion
            return Output.ToString();
        }
        #endregion

        #region All_Categories_Listing
        String Template_All_Categories_Listing()
        {
            StringBuilder Output = new StringBuilder();

            foreach (Category _category in internal_vcrscheduler.Category_Processor.Categories)
            {
                Output.Append("<option value=\"" + _category.Name + "\">" + _category.Name + "</option>");
            }
            return Output.ToString();
        }
        #endregion

        #region Template_All_Category_Searchterms
        String Template_All_Category_Searchterms(Category tempCategory)
        {
            StringBuilder Output = new StringBuilder();

            foreach (String searchterm in tempCategory.SearchTerms)
            {
                Output.Append("<option value=\"" + searchterm+ "\">" + searchterm+ "</option>");
            }
            return Output.ToString();
        }
        #endregion

        #region RecordedListing

        #region Render One Line Listing
        /// <summary>
        /// Actually renders just one line of the recording table, based on the Recording_Entry Parameter
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="recording_entry"></param>
        /// <returns></returns>
        StringBuilder RenderOneLine_Template_Recorded_Listing(StringBuilder Input, Recording recording_entry, String LineTemplate, String Username)
        {
            StringBuilder Output = Input;

            String newLine = LineTemplate;

            // we take that one template line and fill in the placeholders...

            while (newLine.Contains("%createdby%"))
            {
                newLine = newLine.Replace("%createdby%", recording_entry.createdby);
            }

            while (newLine.Contains("%playcount%"))
            {
                // Playcount/NEW!
                if (recording_entry.PlayCount == 0)
                    newLine = newLine.Replace("%playcount%", "<img src=\"images/neu.png\" border=\"0\" alt=\"new recording\"/>");
                else
                    newLine = newLine.Replace("%playcount%", recording_entry.PlayCount.ToString());
            }

            while (newLine.Contains("%recording_status%"))
            {
                if (recording_entry.CurrentlyRecording) newLine = newLine.Replace("%recording_status%", "<img src=\"images/recordbutton.png\" border=\"0\" alt=\"Status: recording\" />");
                else
                {
                    if ((recording_entry.isDaily) | (recording_entry.isMonthly) | (recording_entry.isWeekly))
                        newLine = newLine.Replace("%recording_status%", "<img src=\"images/waitbutton_repeat.png\" border=\"0\" alt=\"Status: wait\"/>");
                    else
                        if (recording_entry.isAutomaticEPGRecording)
                            newLine = newLine.Replace("%recording_status%", "<img src=\"images/waitbutton_automatic.png\" border=\"0\" alt=\"Status: wait\"/>");
                        else
                            newLine = newLine.Replace("%recording_status%", "<img src=\"images/waitbutton.png\" border=\"0\" alt=\"Status: wait\"/>");
                }
            }

            while (newLine.Contains("%starts_at%"))
            {
                if (!recording_entry.isAutomaticEPGRecording)
                {
                    newLine = newLine.Replace("%starts_at%", recording_entry.StartsAt.ToShortDateString() + " " + recording_entry.StartsAt.ToShortTimeString());
                }
                else
                {
                    if (recording_entry.StartsAt.Ticks == 0)
                    {
                        newLine = newLine.Replace("%starts_at%", "n/a");
                    }
                    else
                    {
                        newLine = newLine.Replace("%starts_at%", recording_entry.StartsAt.ToShortTimeString());
                    }
                }
            }

            while (newLine.Contains("%ends_at%"))
            {
                if (!recording_entry.isAutomaticEPGRecording)
                {
                    newLine = newLine.Replace("%ends_at%", recording_entry.EndsAt.ToShortDateString() + " " + recording_entry.EndsAt.ToShortTimeString());
                }
                else
                {
                    if (recording_entry.EndsAt.Ticks == 0)
                    {
                        newLine = newLine.Replace("%ends_at%", "n/a");
                    }
                    else
                    {
                        newLine = newLine.Replace("%ends_at%", recording_entry.EndsAt.ToShortTimeString());
                    }
                }
            }

            while (newLine.Contains("%runtime%"))
            {
                if (!recording_entry.isAutomaticEPGRecording)
                {
                    // Runtime in Minutes
                    TimeSpan runtimeticks = new TimeSpan((recording_entry.EndsAt.Ticks - recording_entry.StartsAt.Ticks));

                    newLine = newLine.Replace("%runtime%", Convert.ToString(Convert.ToInt32(runtimeticks.TotalMinutes)) + " mins");
                }
                else
                {
                    newLine = newLine.Replace("%runtime%", recording_entry.AutomaticRecordingLength + " mins");
                }
            }

            while (newLine.Contains("%channel%"))
            {
                // HACK: change that!!
                try
                {
                    if (ChannelAndStationMapper.HasPicture(Convert.ToInt32(recording_entry.Channel)))
                    {
                        newLine = newLine.Replace("%channel%", "<img src=\"" + ChannelAndStationMapper.Number2Picture(Convert.ToInt32(recording_entry.Channel)) + "\" border=\"0\" alt=\"" + ChannelAndStationMapper.Number2Name(Convert.ToInt32(recording_entry.Channel)) + "\" />");
                    }
                    else
                    {
                        newLine = newLine.Replace("%channel%", recording_entry.Channel);
                    }
                }
                catch (Exception)
                {
                    newLine = newLine.Replace("%channel%", recording_entry.Channel);
                }
            }

            while (newLine.Contains("%recording_name%"))
            {
                newLine = newLine.Replace("%recording_name%", recording_entry.Recording_Name);
            }

            // TODO: make it configurable...Internationalization
            // TODO: make the servername configurable...

            while (newLine.Contains("%categories%"))
            {
                List<Category> categories = internal_vcrscheduler.Category_Processor.AutomaticCategoriesForRecording(recording_entry);

                if (categories == null)
                    newLine = newLine.Replace("%categories%", "n/a");
                else
                {
                    bool morethanone = false;

                    StringBuilder categoryOutput = new StringBuilder();
                    // TODO: possible character encoding problem here....
                    foreach (Category category in categories)
                    {
                        if (!morethanone)
                            categoryOutput.Append("<a href=\"/recordings.html?categoryfilter=" + HttpUtility.UrlEncode(category.Name) + "\">" + category.Name + "</a>");
                        else
                            categoryOutput.Append(", <a href=\"/recordings.html?categoryfilter=" + HttpUtility.UrlEncode(category.Name) + "\">" + category.Name + "</a>");
                        morethanone = true;
                    }
                    newLine = newLine.Replace("%categories%", categoryOutput.ToString());
                }
            }

            while (newLine.Contains("%recording_id%"))
            {
                newLine = newLine.Replace("%recording_id%", recording_entry.Recording_ID.ToString());
            }

            while (newLine.Contains("%played_percentage%"))
            {
                if ( (recording_entry.LastStopPosition(Username) != 0) && (recording_entry.FileSize != 0) )
                {
                    int percentage = Convert.ToInt32(((float)recording_entry.LastStopPosition(Username) / (float)recording_entry.FileSize) * 100);

                    newLine = newLine.Replace("%played_percentage%",percentage.ToString());
                }
                else
                    newLine = newLine.Replace("%played_percentage%", "0");
            }


            Output.Append(newLine);

            return Output;
        }
        #endregion

        /// <summary>
        /// this generates the Record Listing HTML Sourcecode
        /// </summary>
        /// <returns>Record Listing Sourcecode</returns>
        String Template_Recorded_Listing(Category FilterCategory, bool SortAscending, String LineTemplate, String Username)
        {
            StringBuilder Output = new StringBuilder();
            /*
            // start the table
            Output.Append("	<table id=\"TABLE2\" width=\"100%\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">	<tr>		<td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 79px;\">            <strong>playcount</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); width: 169px; background-repeat: repeat-x; height: 21px; text-align: center\">            ");

            #region SortRecordedAt
            // this is the recorded-at sort button...
            // TODO: at this time this only respects the Filtercategory functionality...add more/general

            if (SortAscending)
                Output.Append("<a href=\"/recordings.html?SortDescending");
            else
                Output.Append("<a href=\"/recordings.html?SortAscending");
                

            if (FilterCategory != null)
                Output.Append("&CategoryFilter="+HttpUtility.UrlEncode(FilterCategory.Name));

            Output.Append("\"><strong>recorded at</strong></a>");
            #endregion

            Output.Append("</td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); width: 75px; background-repeat: repeat-x; height: 21px; text-align: center\">            <strong>duration</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 64px;\">            <strong>channel</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 278px;\">            <strong>name</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 129px;\">            <strong>category</strong></td>  	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 79px;\">        </td>	</tr>	");
            // OLD Output.Append("<table width=\"90%\"><tr style=\"background-color: #93939a;\"><td align=\"center\">Time</td><td align=\"center\">Sender</td><td align=\"center\">Name</td></tr>");
            */
            if (FilterCategory == null)
            {
                // when we don't filter anything
                if (internal_vcrscheduler.doneRecordings.Count > 0)
                {
                    List<Recording> sortedDoneRecordingList;
                    lock (internal_vcrscheduler.doneRecordings.SyncRoot)
                    {
                        // TODO: add ascending/descending setting + reimplement the sorting algorithm
                        sortedDoneRecordingList = Sorter.SortRecordingTable(internal_vcrscheduler.doneRecordings, SortAscending);
                    }

                    foreach (Recording recording_entry in sortedDoneRecordingList)
                    {
                        Output = RenderOneLine_Template_Recorded_Listing(Output, recording_entry, LineTemplate, Username);
                    }
                }
            }
            else
            {
                // when we should filter...
                if (internal_vcrscheduler.doneRecordings.Count > 0)
                {
                    List<Recording> sortedDoneRecordingList;
                    lock (internal_vcrscheduler.doneRecordings.SyncRoot)
                    {
                        // TODO: add ascending/descending setting + reimplement the sorting algorithm
                        sortedDoneRecordingList = Sorter.SortRecordingTable(internal_vcrscheduler.doneRecordings, SortAscending);
                    }

                    foreach (Recording recording_entry in sortedDoneRecordingList)
                    {
                        if (internal_vcrscheduler.Category_Processor.isRecordingInCategory(recording_entry,FilterCategory))
                            Output = RenderOneLine_Template_Recorded_Listing(Output, recording_entry, LineTemplate, Username);
                    }
                }

            }
            // close the table html tag
            //Output.Append("</table>");

            if ((internal_vcrscheduler.Recordings.Count == 0) && (internal_vcrscheduler.doneRecordings.Count == 0))
            {
                Output.Remove(0, Output.Length);
                Output.Append("No Recordings...");
            }

            return Output.ToString();
        }
        #endregion

        #region RecordingListing
        /// <summary>
        /// this generates the Record Listing HTML Sourcecode
        /// </summary>
        /// <returns>Record Listing Sourcecode</returns>
        String Template_Recording_Listing(string LineTemplate, String Username)
        {
            StringBuilder Output = new StringBuilder();

            // BUG: when there's nothing currently recording the header of the table alone is shown...fix that

            // start the table

            // TODO: add a progress bar to each Status
            //Output.Append("<table id=\"TABLE1\" width=\"100%\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\">	<tr>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 55px;\">            <strong>status</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); width: 169px; background-repeat: repeat-x; height: 21px; text-align: center\">            <strong>starts</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); width: 171px; background-repeat: repeat-x; height: 21px; text-align: center\">            <strong>ends</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 106px;\">            <strong>channel</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 277px;\">            <strong>name</strong></td>	    <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x; height: 21px; text-align: center; width: 79px;\">            <strong>category</strong></td>        <td style=\"background-image: url(images/Table_Headerbar_Tile.png); background-repeat: repeat-x;            height: 21px; text-align: center; width: 60px;\"></td>	</tr>");
            // OLD Output.Append("<table width=\"90%\"><tr style=\"background-color: #93939a;\"><td style=\"width:50px\" align=\"center\">Status</td><td align=\"center\">Time</td><td align=\"center\">Sender</td><td align=\"center\">Name</td></tr>");

            if (internal_vcrscheduler.doneRecordings.Count > 0)
            {
                List<Recording> sortedDoneRecordingList;
                lock (internal_vcrscheduler.doneRecordings.SyncRoot)
                {
                    // TODO: add ascending/descending setting + reimplement the sorting algorithm
                    sortedDoneRecordingList = Sorter.SortRecordingTable(internal_vcrscheduler.doneRecordings, true);
                }

                foreach (Recording recording_entry in sortedDoneRecordingList)
                {
                    if (recording_entry.CurrentlyRecording == true)
                    {
                        Output = RenderOneLine_Template_Recorded_Listing(Output, recording_entry, LineTemplate, Username);
                    }
                }

            }
            // just work when there is work to do...
            if (internal_vcrscheduler.Recordings.Count > 0)
            {
                List<Recording> sortedDoneRecordingList;
                lock (internal_vcrscheduler.Recordings.SyncRoot)
                {
                    // TODO: add ascending/descending setting + reimplement the sorting algorithm
                    sortedDoneRecordingList = Sorter.SortRecordingTable(internal_vcrscheduler.Recordings, true);
                }
                foreach (Recording recording_entry in sortedDoneRecordingList)
                {
                    //Output.Append("<tr>            <td style=\"text-align: center; width: 60px;\">            <img src=\"images/waitbutton.png\" border=\"0\" /></td>            <td style=\"text-align: center; width: 169px;\">");

                    // OLD Output.Append("<tr style=\"background-color: #a29ca2;\"><td style=\"background-color:red;width:50px\"></td><td align=\"center\" style=\"width:150px\">");

                    Output = RenderOneLine_Template_Recorded_Listing(Output, recording_entry, LineTemplate, Username);
                }
            }

            //Output.Append("</table>");

            if ((internal_vcrscheduler.Recordings.Count == 0) && (internal_vcrscheduler.doneRecordings.Count == 0))
            {
                Output.Remove(0, Output.Length);
                Output.Append("No Recordings...");
            }

            return Output.ToString();
        }
        #endregion

        #region ConsoleOutput
        /// <summary>
        /// this generates the Record Listing HTML Sourcecode
        /// </summary>
        /// <returns>Record Listing Sourcecode</returns>
        String Template_Console_Output()
        {
            StringBuilder Output = new StringBuilder();

            String[] ConsoleOutput = ConsoleOutputLogger.GetLoggedLines();

            Output.Append("<pre>");

            foreach (String line in ConsoleOutput)
            {
                Output.Append(line + "\n");
            }
            Output.Append("</pre>");
            return Output.ToString();
        }
        #endregion

        #endregion
    }
}
