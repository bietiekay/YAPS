using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS.tuxbox
{
    [Serializable]
    public class image
    {
        public String version;
        public String url;
        public String comment;
        public String catalog;
    }

    [Serializable]
    public class boxinfo
    {
        public image image;
        public String firmware;
        public String fpfirmware;
        public String webinterface;
        public String model;
        public String manufacturer;
        public String processor;
        public String usbstick;
        public String disk;

        public boxinfo()
        {
            image = new image();
        }
    }
}
