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
                                if (recording_entry.isAutomaticEPGRecording)
                                {
                                    #region Automatic Recordings
                                    foreach (EPG_Event_Entry currentlyRunningEvent in internal_http_server_object.EPGProcessor.CurrentlyRunningEvents)
                                    {
                                        if (!currentlyRunningEvent.isRecorded)
                                        {
                                            // if a channel is set, only record if the new event is on that channel
                                            if (recording_entry.Channel != "")
                                            {
                                                if (recording_entry.Channel != ChannelAndStationMapper.Name2Number(ChannelAndStationMapper.ServiceID2Name(currentlyRunningEvent.Service)).ToString())
                                                {
                                                    //ConsoleOutputLogger.WriteLine(recording_entry.Channel+" != "+ChannelAndStationMapper.Name2Number(ChannelAndStationMapper.ServiceID2Name(currentlyRunningEvent.Service)).ToString());
                                                    continue;
                                                }
                                            }
                                            // 15.03.2008 - 10:07 Debug: 633411724556096250 (DateTime.Now)
                                            // 15.03.2008 - 10:07 Debug: 633411690000000000 (DateTime.StartTime)
                                            // if the start time is set, look if this is correct
                                            if (recording_entry.StartsAt.Ticks != 0)
                                            {
                                                DateTime newStartsAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, recording_entry.StartsAt.Hour, recording_entry.StartsAt.Minute, DateTime.Now.Second, 0);

                                                // if the recording hasn't even started yet
                                                if (newStartsAt.Ticks >= DateTime.Now.Ticks)
                                                {
                                                    //ConsoleOutputLogger.WriteLine(newStartsAt.ToShortTimeString()+" >= "+DateTime.Now.ToShortTimeString());
                                                    continue;
                                                }
                                            }
                                            // if the end time is set, look if this is correct
                                            if (recording_entry.EndsAt.Ticks != 0)
                                            {
                                                DateTime newEndsAt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, recording_entry.EndsAt.Hour, recording_entry.EndsAt.Minute, DateTime.Now.Second, 0);

                                                if (newEndsAt.Ticks <= DateTime.Now.Ticks)
                                                {
                                                    //ConsoleOutputLogger.WriteLine(newEndsAt.ToShortTimeString() + " <= " + DateTime.Now.ToShortTimeString());
                                                    continue;
                                                }
                                            }

                                            // check if the time is right...

                                            #region only handle this event if it's not already recorded...
                                            foreach (String Keyword in recording_entry.AutomaticEPGRecordingKeywords)
                                            {
                                                //ConsoleOutputLogger.WriteLine(Keyword +" -> "+currentlyRunningEvent.ShortDescription.Name);
                                                if (currentlyRunningEvent.ShortDescription.Name.ToUpper().IndexOf(Keyword.ToUpper()) != -1)
                                                {
                                                    // we found the substring
                                                    ConsoleOutputLogger.WriteLine("Automatic Recording matched Keyword: " + Keyword);
                                                    ConsoleOutputLogger.WriteLine("Creating new Recording " + currentlyRunningEvent.ShortDescription.Name + " on Channel " + ChannelAndStationMapper.ServiceID2Name(currentlyRunningEvent.Service));

                                                    // only record if either the channel is the channel of the automatic recording or the channel doesn't matter ("")
                                                    //if ( (recording_entry.Channel != "") || (recording_entry.Channel == ChannelAndStationMapper.Name2Number(ChannelAndStationMapper.ServiceID2Name(currentlyRunningEvent.Service)).ToString()))
                                                    //{

                                                    // we're recording this!
                                                    currentlyRunningEvent.isRecorded = true;

                                                    // since we don't know how long this recording will be, we set a maximum time of 4 hours (240 minutes9
                                                    Recording newRecording = new Recording();

                                                    newRecording.Channel = ChannelAndStationMapper.Name2Number(ChannelAndStationMapper.ServiceID2Name(currentlyRunningEvent.Service)).ToString();
                                                    newRecording.createdby = recording_entry.createdby;
                                                    newRecording.Comment = currentlyRunningEvent.ShortDescription.Text;
                                                    newRecording.Categories = recording_entry.Categories;
                                                    newRecording.AdInPosition = recording_entry.AdInPosition;
                                                    newRecording.AdOutPosition = recording_entry.AdOutPosition;
                                                    newRecording.EndPosition = recording_entry.EndPosition;
                                                    newRecording.Episode = recording_entry.Episode;
                                                    newRecording.EpisodeTitle = recording_entry.EpisodeTitle;
                                                    newRecording.HoldingTime = recording_entry.HoldingTime;
                                                    newRecording.isAutomaticEPGRecording = false;
                                                    newRecording.wasAutomaticEPGRecording = true;
                                                    newRecording.Recording_Name = recording_entry.Recording_Name;
                                                    newRecording.Username = recording_entry.Username;
                                                    newRecording.Season = recording_entry.Season;
                                                    newRecording.Week = recording_entry.Week;
                                                    newRecording.Year = recording_entry.Year;
                                                    newRecording.StartsAt = DateTime.Now;
                                                    newRecording.EndsAt = DateTime.Now.AddMinutes(recording_entry.AutomaticRecordingLength);

                                                    lock (doneRecordings.SyncRoot)
                                                    {
                                                        doneRecordings.Add(newRecording.Recording_ID, newRecording);
                                                    }
                                                    // fire up the recorder... "true" because we're an recorder and not a streamer
                                                    VCRandStreaming HReq = new VCRandStreaming(true, newRecording, internal_http_server_object);

                                                    // tell the console that we're going to record something right now...
                                                    ConsoleOutputLogger.WriteLine("Record started at " + newRecording.StartsAt.ToShortTimeString() + " - Name: " + newRecording.Recording_Name);
                                                    Settings.NumberOfRecordings++;
                                                    // we're recording
                                                    newRecording.CurrentlyRecording = true;

                                                    // call the Handler and lets get back to our job of scheduling...
                                                    HReq.HandleVCR(ChannelAndStationMapper.Number2Data(Convert.ToInt32(newRecording.Channel)), internal_http_server_object);
                                                    continue;
                                                    //}
                                                }
                                            }
                                            #endregion
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    // TODO: maybe we should also check if the EndsAt is also reached; so we do not start recordings that already passed by
                                    if ((recording_entry.StartsAt.Ticks - DateTime.Now.Ticks) <= 0)
                                    {
                                        // TODO: Recording "done" list handling...

                                        lock (doneRecordings.SyncRoot)
                                        {
                                            // move the recording to the "done" list
                                            doneRecordings.Add(recording_entry.Recording_ID, recording_entry);
                                        }
                                        lock (Recordings.SyncRoot)
                                        {
                                            // remove the recording from the todo-Recordings List
                                            Recordings.Remove(recording_entry.Recording_ID);
                                        }

                                        #region Reoccuring Recordings
                                        // everything regarding reoccuring event handling takes place here

                                        // first check if we have to do anything
                                        // TODO: we're currently only checking for "each", not for anything else

                                        if (recording_entry.isDaily)
                                        {
                                            int StartDay = CalcDayOfWeekNumber(DateTime.Now.DayOfWeek);
                                            int Counter = 0;

                                            bool done2 = false;

                                            while (!done2)
                                            {
                                                StartDay++;
                                                Counter++;

                                                if (StartDay == 7)
                                                    StartDay = 0;

                                                if (recording_entry.Week[StartDay] == true)
                                                    done2 = true;
                                            }

                                            Counter = Counter * recording_entry.isEach;

                                            Recording newRecording = recording_entry.Clone();

                                            newRecording.StartsAt = newRecording.StartsAt.AddDays(Convert.ToDouble(Counter));
                                            newRecording.EndsAt = newRecording.EndsAt.AddDays(Convert.ToDouble(Counter));

                                            lock (Recordings.SyncRoot)
                                            {
                                                Recordings.Add(newRecording.Recording_ID, newRecording);
                                            }
                                        }

                                        #endregion

                                        // fire up the recorder... "true" because we're an recorder and not a streamer
                                        VCRandStreaming HReq = new VCRandStreaming(true, recording_entry, internal_http_server_object);

                                        // tell the console that we're going to record something right now...
                                        ConsoleOutputLogger.WriteLine("Record started at " + recording_entry.StartsAt.ToShortTimeString() + " - Name: " + recording_entry.Recording_Name);
                                        Settings.NumberOfRecordings++;
                                        // we're recording
                                        recording_entry.CurrentlyRecording = true;

                                        // call the Handler and lets get back to our job of scheduling...
                                        HReq.HandleVCR(ChannelAndStationMapper.Number2Data(Convert.ToInt32(recording_entry.Channel)), internal_http_server_object);
                                        break;
                                    }
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
        private Int32 CalcDayOfWeekNumber(DayOfWeek Input)
        {
            if (Input == DayOfWeek.Monday)
                return 0;
            if (Input == DayOfWeek.Tuesday)
                return 1;
            if (Input == DayOfWeek.Wednesday)
                return 2;
            if (Input == DayOfWeek.Thursday)
                return 3;
            if (Input == DayOfWeek.Friday)
                return 4;
            if (Input == DayOfWeek.Saturday)
                return 5;
            if (Input == DayOfWeek.Sunday)
                return 6;
            return 0;
        }
    }
    #endregion
}
