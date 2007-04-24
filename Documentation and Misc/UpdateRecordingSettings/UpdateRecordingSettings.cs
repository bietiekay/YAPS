using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    // This is a Utility to update the Recording Settings Configuration Files from one version to another...
    class UpdateRecordingSettings
    {
        static void Main(string[] args)
        {
            FileStream fs;

            Hashtable old_Recordings;
            Hashtable old_DoneRecordings;
            Hashtable Recordings = new Hashtable();
            Hashtable DoneRecordings = new Hashtable();

            #region Load

            #region Recordings
            fs = new FileStream("YAPS.Recordings.dat", FileMode.Open, FileAccess.Read);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                old_Recordings = (Hashtable)bf.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
            #endregion

            #region DoneRecordings
            fs = new FileStream("YAPS.DoneRecordings.dat", FileMode.Open, FileAccess.Read);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                old_DoneRecordings = (Hashtable)bf.Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
            #endregion

            #endregion

            #region Convert
            foreach (RecordingV2 recording_entry in old_Recordings.Values)
            {
                YAPS.Recording newRecordingEntry = new Recording();

                //newRecordingEntry.Categories = recording_entry.Categories;
                newRecordingEntry.Channel = recording_entry.Channel;
                newRecordingEntry.CurrentlyRecording = recording_entry.CurrentlyRecording;
                newRecordingEntry.EndsAt = recording_entry.EndsAt;
                newRecordingEntry.isDaily = recording_entry.isDaily;
                newRecordingEntry.isEach = recording_entry.isEach;
                newRecordingEntry.isMonthly = recording_entry.isMonthly;
                newRecordingEntry.isWeekly = recording_entry.isWeekly;
                newRecordingEntry.LastTimePlayed = recording_entry.LastTimePlayed;
                newRecordingEntry.Password = recording_entry.Password;
                newRecordingEntry.PlayCount = recording_entry.PlayCount;
                newRecordingEntry.Recording_Filename = recording_entry.Recording_Filename;
                newRecordingEntry.Recording_ID = recording_entry.Recording_ID;
                newRecordingEntry.Recording_Name = recording_entry.Recording_Name;
                newRecordingEntry.StartsAt = recording_entry.StartsAt;
                newRecordingEntry.Username = recording_entry.Username;
                newRecordingEntry.Week = recording_entry.Week;

                newRecordingEntry.Season = 0;
                newRecordingEntry.Episode = 0;
                newRecordingEntry.EpisodeTitle = "";
                newRecordingEntry.FileSize = 0;
                newRecordingEntry.Comment = "";
                newRecordingEntry.LastStoppedPosition = 0;
                newRecordingEntry.Year = 0;

                Recordings.Add(newRecordingEntry.Recording_ID, newRecordingEntry);
            }

            foreach (RecordingV2 recording_entry in old_DoneRecordings.Values)
            {
                YAPS.Recording newRecordingEntry = new Recording();

                //newRecordingEntry.Categories = recording_entry.Categories;
                newRecordingEntry.Channel = recording_entry.Channel;
                newRecordingEntry.CurrentlyRecording = recording_entry.CurrentlyRecording;
                newRecordingEntry.EndsAt = recording_entry.EndsAt;
                newRecordingEntry.isDaily = recording_entry.isDaily;
                newRecordingEntry.isEach = recording_entry.isEach;
                newRecordingEntry.isMonthly = recording_entry.isMonthly;
                newRecordingEntry.isWeekly = recording_entry.isWeekly;
                newRecordingEntry.LastTimePlayed = recording_entry.LastTimePlayed;
                newRecordingEntry.Password = recording_entry.Password;
                newRecordingEntry.PlayCount = recording_entry.PlayCount;
                newRecordingEntry.Recording_Filename = recording_entry.Recording_Filename;
                newRecordingEntry.Recording_ID = recording_entry.Recording_ID;
                newRecordingEntry.Recording_Name = recording_entry.Recording_Name;
                newRecordingEntry.StartsAt = recording_entry.StartsAt;
                newRecordingEntry.Username = recording_entry.Username;
                newRecordingEntry.Week = recording_entry.Week;

                newRecordingEntry.Season = 0;
                newRecordingEntry.Episode = 0;
                newRecordingEntry.EpisodeTitle = "";
                newRecordingEntry.FileSize = 0;
                newRecordingEntry.Comment = "";
                newRecordingEntry.LastStoppedPosition = 0;
                newRecordingEntry.Year = 0;

                DoneRecordings.Add(newRecordingEntry.Recording_ID, newRecordingEntry);
            }
            #endregion

            #region Save
            #region Recordings
            fs = new FileStream("YAPS.Recordings.dat", FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, Recordings);
            }
            finally
            {
                fs.Close();
            }
            #endregion

            #region DoneRecordings
            fs = new FileStream("YAPS.DoneRecordings.dat", FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, DoneRecordings);
            }
            finally
            {
                fs.Close();
            }
            #endregion
            #endregion
        }
    }
}
