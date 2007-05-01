using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace YAPS
{
    #region VCR Scheduler
    /// <summary>
    /// Implements the scheduler which starts the recording...
    /// </summary>
    public class VCRScheduler
    {
        // a simple hashtable which holds all the recordings, done and todo...
        public Hashtable Recordings;
        public Hashtable doneRecordings;
        public Settings Settings;
        public SettingsProcessor Configuration;
        public CategoryProcessor Category_Processor;
        bool done;
        private YAPS.HttpServer internal_http_server_object;

        // first of all we spawn a thread that takes care of the timers...
        public VCRScheduler(YAPS.HttpServer Http_Server_Object)
        {
            Recordings = new Hashtable();
            doneRecordings = new Hashtable();
            done = false;
            internal_http_server_object = Http_Server_Object;
            Category_Processor = new CategoryProcessor(internal_http_server_object);
        }

        /// <summary>
        /// when this is called the Scheduler Thread will shut down the next turnaround
        /// </summary>
        public void youreDone()
        {
            done = true;
        }

        /// <summary>
        /// the actual Scheduler
        /// </summary>
        /// <param name="internal_http_processor_instance">we need this because the handler class needs it to get to the multicast and receiver lists...does</param>
        public void RecordingScheduler()
        {
            // tell the HTTP Server that we're up'n'running
            internal_http_server_object.vcr_scheduler = this;

            lock (doneRecordings.SyncRoot)
            {
                // check if we have some undone recordings in the queue that need to be set to "done"...
                foreach (Recording recording_entry in doneRecordings.Values)
                {
                    if (recording_entry.CurrentlyRecording)
                    {
                        recording_entry.CurrentlyRecording = false;
                        ConsoleOutputLogger.WriteLine("Obviously the recording " + recording_entry.Recording_Name + " did not finish properly.");
                    }

                    #region HACK: Create playlist files on launch...
                    // normally commented section, use only of you know what you're doing
                    /*using (StreamWriter sw = new StreamWriter(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry)))
                    {
                        // Add some text to the file.
                        sw.Write(Settings.HTTP_URL + "/" + recording_entry.Recording_Filename);
                        sw.Close();
                    }
                    File.SetLastWriteTime(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry), recording_entry.EndsAt);
                    File.SetCreationTime(XBMCPlaylistFilesHelper.generatePlaylistFilename(recording_entry), recording_entry.EndsAt);*/
                    #endregion

                }
            }


            ConsoleOutputLogger.WriteLine("VCR Scheduler up and running...");

            // as long as we're not done, check for upcoming recordings...
            while ((true) && (!done))
            {
                try
                {
                    if (Recordings.Count > 0)
                    {
                        lock (Recordings.SyncRoot)
                        {
                            foreach (Recording recording_entry in Recordings.Values)
                            {
                                // TODO: maybe we should also check if the EndsAt is also reached; so we do not start recordings that already passed by
                                if ((recording_entry.StartsAt.Ticks - DateTime.Now.Ticks) <= 0)
                                {
                                    // TODO: Recording "done" list handling...

                                    // move the recording to the "done" list
                                    doneRecordings.Add(recording_entry.Recording_ID, recording_entry);
                                    // remove the recording from the todo-Recordings List
                                    Recordings.Remove(recording_entry.Recording_ID);

                                    // fire up the recorder... "true" because we're an recorder and not a streamer
                                    VCRandStreaming HReq = new VCRandStreaming(true, recording_entry,internal_http_server_object);

                                    // tell the console that we're going to record something right now...
                                    ConsoleOutputLogger.WriteLine("Record started at " + recording_entry.StartsAt.ToShortTimeString() + " - Name: " + recording_entry.Recording_Name);

                                    // we're recording
                                    recording_entry.CurrentlyRecording = true;

                                    // call the Handler and lets get back to our job of scheduling...
                                    HReq.HandleVCR(ChannelAndStationMapper.Number2Data(Convert.ToInt32(recording_entry.Channel)), internal_http_server_object);
                                    break;
                                }
                            }
                        }
                    }
                    // wait another 1000 mseconds, then look again for recordings...
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    // well, most likely we will run into some sync issues with the hashtables...but
                    // since I don't want to deal with this right now I am just going to ignore it..
                    // does work, you should try that in real life as well.

                    ConsoleOutputLogger.WriteLine("Scheduler Error: " + e.Message);
                }
            }
        }
    }
    #endregion
}
