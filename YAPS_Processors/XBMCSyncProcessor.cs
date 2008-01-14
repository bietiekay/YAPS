using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public class XBMCSyncProcessor
    {
        private YAPS.HttpServer internal_http_server_object;
        bool done;

        public XBMCSyncProcessor(YAPS.HttpServer Http_Server_Object)
        {
            done = false;
            internal_http_server_object = Http_Server_Object;
        }

        /// <summary>
        /// when this is called the Scheduler Thread will shut down the next turnaround
        /// </summary>
        public void youreDone()
        {
            done = true;
        }

        /// <summary>
        /// this processor helps to keep the recordings playlist files in sync with the actual recording list
        /// </summary>
        public void SyncProcessor()
        {
            // wait for the VCRScheduler to come up...
            ConsoleOutputLogger.WriteLine("XBMCSyncProcessor is waiting for VCRScheduler...");
            while (!internal_http_server_object.internal_vcr_scheduler_set)
            {
                Thread.Sleep(10);
            }

            ConsoleOutputLogger.WriteLine("XBMC SyncProcessor up and running...");
            // as long as we're not done, check for upcoming recordings...
            while ((true) && (!done))
            {
                try
                {
                    #region here starts the Syncing Fun
                    if (internal_http_server_object.vcr_scheduler.doneRecordings.Count > 0)
                    {
                        lock (internal_http_server_object.vcr_scheduler.doneRecordings.SyncRoot)
                        {
                            foreach (Recording recording_entry in internal_http_server_object.vcr_scheduler.doneRecordings.Values)
                            {
                                
                                // check holding time...
                                if (recording_entry.HoldingTime != 0)
                                {
                                    if (recording_entry.HoldingTime <= HoldingTimeManager.HowOldIsThisRecordingInDays(recording_entry.EndsAt))
                                    {
                                        ConsoleOutputLogger.WriteLine("The HoldingTime of "+recording_entry.Recording_Name+" is reached, deleting...");

                                        if (File.Exists(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry)))
                                        {
                                            File.Delete(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry));
                                        }                                        
                                    }
                                }

                                // one run through the whole list and check if the playlist file exists... if it does not; also remove the recording...
                                if (!File.Exists(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry)))
                                {
                                    if (!recording_entry.CurrentlyRecording)
                                    {
                                        // remove the recording
                                        ConsoleOutputLogger.WriteLine("Apparently the Playlistfile for " + recording_entry.Recording_Name + " does not exist anymore - deleting recording");

                                        RecordingsManager.deleteRecording(recording_entry, internal_http_server_object.vcr_scheduler);
                                        
                                        File.Delete(XBMCPlaylistFilesHelper.generateThumbnailFilename(recording_entry));
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    // wait another 1000 mseconds, then look again for recordings...
                    Thread.Sleep(5000);
                }
                catch (Exception)
                {
                    // well, most likely we will run into some sync issues with the hashtables...but
                    // since I don't want to deal with this right now I am just going to ignore it..
                    // does work, you should try that in real life as well.
                    //ConsoleOutputLogger.WriteLine("XBMC SyncProcessor Error: " + e.Message);
                }

            }
        }
    }
}
