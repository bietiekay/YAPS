using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    [Serializable]
    public class StationAndChannel
    {
        public String ChannelName;
        public Int32 ChannelNumber;           // legacy
        public String ChannelPictureURL;
        public ushort ServiceID;

        public String MulticastIP;
        public String MulticastPort;

        public StationAndChannel()
        {
            ChannelName = "";
            ChannelNumber = -1;
            ChannelPictureURL = "";
            ServiceID = 0;
            MulticastPort = "";
            MulticastIP = "";
        }
    }
}
