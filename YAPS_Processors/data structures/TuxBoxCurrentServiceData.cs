using System;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace YAPS.tuxbox
{
    [Serializable]
    public class currentservicedata_service
    {
        public String name;
        public String reference;
    }
    [Serializable]
    public class channel
    {
        public String pid;
        public Int32 selected;
        public String Name;
    }
    [Serializable]
    public class currentservicedata_event
    {
        public String date;
        public String time;
        public String start;
        public String duration;
        public String description; // which is actually the name
        public String details;
    }
    [Serializable]
    public class currentservicedata
    {
        public currentservicedata_service service;

        public List<channel> audio_channels;
        public currentservicedata_event current_event;
        public currentservicedata_event next_event;

        public currentservicedata()
        {
            service = new currentservicedata_service();

            audio_channels = new List<channel>();

            current_event = new currentservicedata_event();
            next_event = new currentservicedata_event();
        }
    }

}
