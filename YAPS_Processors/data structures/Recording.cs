using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace YAPS
{
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

        public bool isWeekly; // weekly
        public bool isDaily; // daily
        public bool isMonthly; // monthly

        public bool[] Week;

        public int isEach; // each 1st, 2nd, 3rd....

        public String createdby; // who created this recording (Username)

        #region reserved for future extensions
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
        public long LastStoppedPosition;
        public long StartPosition;
        public long EndPosition;

        public List<long> AdInPosition;
        public List<long> AdOutPosition;


        #endregion

        public Recording()
        {
            Recording_ID = Guid.NewGuid();
            Categories = new List<Category>();
            CurrentlyRecording = false;
            isWeekly = false;
            isDaily = false;
            isMonthly = false;
            isEach = 1;
            createdby = "";
            Week = new bool[7];
        }
    }
}