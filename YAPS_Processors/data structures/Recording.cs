using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace YAPS
{
    [Serializable]
    public class UserStopPosition
    {
        public long LastStoppedPosition;
        public String WatchedBy;

        public UserStopPosition()
        {
        }

        public UserStopPosition(String Username, long Position)
        {
            LastStoppedPosition = Position;
            WatchedBy = Username;
        }
    }

    /// <summary>
    /// holds all the data of one particular recording
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(Recording))]
    public class Recording
    {
        public String Recording_Name;
        public String Recording_Filename;
        public String Channel;
        public DateTime StartsAt;
        public DateTime EndsAt;
        public Guid Recording_ID;
        public bool CurrentlyRecording;

        public bool isAutomaticEPGRecording;
        public bool wasAutomaticEPGRecording;
        public List<String> AutomaticEPGRecordingKeywords;
        public Int32 AutomaticRecordingLength;

        public bool isWeekly; // weekly
        public bool isDaily; // daily
        public bool isMonthly; // monthly

        public bool[] Week;

        public int isEach; // each 1st, 2nd, 3rd....

        public String createdby; // who created this recording (Username)

        public List<UserStopPosition> LastStoppedPositions;

        public Int32 HoldingTime; // the time this recording will be saved...

        // TODO: implement those below...
        public Int32 PlayCount;
        public DateTime LastTimePlayed;

        public String Password;
        public String Username;

        // this does not apply anoymore...please ignore
        public List<Category> Categories;

        // commented out until a recording file converter is available
        // TODO: create a recording file updater/converter

        public long FileSize;
        public string Comment;
        public string EpisodeTitle;
        public long Episode;
        public long Season;
        public long Year;
        public long StartPosition;
        public long EndPosition;

        public List<long> AdInPosition;
        public List<long> AdOutPosition;

        public Recording()
        {
            Recording_ID = Guid.NewGuid();
            Recording_Filename = Recording_ID.ToString();
            Categories = new List<Category>();
            CurrentlyRecording = false;
            isWeekly = false;
            isDaily = false;
            isMonthly = false;
            isEach = 1;
            createdby = "";
            Week = new bool[7];
            LastStoppedPositions = new List<UserStopPosition>();
            AutomaticEPGRecordingKeywords = new List<string>();
            HoldingTime = 0;
        }

        public long LastStopPosition(String Username)
        {
            lock (LastStoppedPositions)
            {
                foreach (UserStopPosition uposition in LastStoppedPositions)
                {
                    if (uposition.WatchedBy == Username)
                        return uposition.LastStoppedPosition;
                }
            }
            return 0;
        }

        public void SetLastStopPosition(String Username, long Position)
        {
            lock (LastStoppedPositions)
            {
                foreach (UserStopPosition uposition in LastStoppedPositions)
                {
                    if (uposition.WatchedBy == Username)
                    {
                        uposition.LastStoppedPosition = Position;
                        return;
                    }
                }
                // we end up here in case we did not find the User
                LastStoppedPositions.Add(new UserStopPosition(Username, Position));
            }
        }


        public Recording Clone()
        {
            Recording Output = new Recording();

            Output.Channel = Channel;
            Output.StartsAt = new DateTime(StartsAt.Ticks);
            Output.EndsAt = new DateTime(EndsAt.Ticks);
            Output.Recording_Name = Recording_Name;
            Output.isWeekly = isWeekly;
            Output.isDaily = isDaily;
            Output.isMonthly = isMonthly;
            Output.isEach = isEach;
            Output.Week = Week;
            Output.createdby = createdby;
            Output.HoldingTime = HoldingTime;
            Output.Username = Username;
            Output.Password = Password;
            Output.Comment = Comment;
            Output.EpisodeTitle = EpisodeTitle;
            Output.Episode = Episode;
            Output.Season = Season;
            Output.Year = Year;

            return Output;
        }
    }
}