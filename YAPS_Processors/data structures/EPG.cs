using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JMS.DVB.EPG;

namespace YAPS
{
    #region EPG Thread Instance data structures
    public class epgThreadInstance
    {
        public Thread EPG_Thread;

        public Hashtable EPG_Events;
        public EPGThread EPG_Thread_Class;
        public MulticastEPGSource EPGDataSource;

        public epgThreadInstance(MulticastEPGSource EDSource, Hashtable EPGEvents)
        {
            EPGDataSource = EDSource;
            EPG_Events = EPGEvents;
            EPG_Thread_Class = new EPGThread(EPGDataSource, EPGEvents);
            EPG_Thread = new Thread(new ThreadStart(EPG_Thread_Class.EPG_Thread));
        }
    }
    #endregion

    #region EPG Events
    [Serializable]
    public class EPG_ShortEvent
    {
        public String Language;
        public String Name;
        public String Text;
        public DescriptorTags Tag;
    }

    [Serializable]
    public class EPG_ExtendedEvent
    {
        public int DescriptorNumber;
        public int LastDescriptorNumber;
        public String Language;
        public String Name;
        public String Text;
        public DescriptorTags Tag;
    }

    [Serializable]
    public class EPG_Event_Entry
    {
        public ushort Service;
        public ushort EventIdentifier;
        public bool FreeCA;
        public String EPG_Provider;
        public DateTime StartTime;
        public DateTime EndTime;
        public Recording AssociatedRecording;

        public bool isRecorded;

        //public Table EIT_Table;
        public EventStatus Status;

        public EPG_ShortEvent ShortDescription;
        public List<EPG_ExtendedEvent> Extended;

        public EPG_Event_Entry()
        {
            isRecorded = false;
            ShortDescription = new EPG_ShortEvent();
            Extended = new List<EPG_ExtendedEvent>();
        }
    }
    #endregion

    #region Station
    [Serializable]
    public class Station
    {
        public String StationName;
        public ushort Service;
    }
    #endregion

    [Serializable]
    public class MulticastEPGSource
    {
        public String IPAddress;
        public int Portnumber;
        public String EPGName;

        /*public MulticastEPGSource(String IP, int Port, String Name)
        {
            IPAddress = IP;
            Portnumber = Port;
            EPGName = Name;
        }*/
    }
}