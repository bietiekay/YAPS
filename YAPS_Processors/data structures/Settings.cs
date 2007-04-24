using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    [Serializable]
    public class Settings
    {
        #region General
        public int Minimum_Free_Filesystem_Space;
        #endregion

        #region VCR specific
        public String VCR_Root_Directory;

        public String VCR_Store_Recordings_Path;
        #endregion

        #region Stream specific
        public bool rewind_after_disconnect;
        public int Multicast_Cached_and_Threaded_Writer_Buffer_Size;
        public string Multicast_IP_Prefix;
        #endregion

        #region Skin specific
        public string Skin;
        #endregion

        #region HTTP Server specific
        public int Maximum_Number_Of_Concurrent_Clients;
        public String HTTP_Root_Directory;
        public String HTTP_IPAdress;   // defaults to 0.0.0.0
        public int HTTP_Port;
        public int HTTP_Buffer_Size;

        public string HTTP_URL
        {
            set
            {
            }
            //Accessor function get
            get
            {
                if (HTTP_Port == 80)
                    return "http://"+ HTTP_IPAdress;
                else
                    return "http://" + HTTP_IPAdress + ":"+ HTTP_Port;
            }
        }

        #endregion

        #region multicast EPG specific
        public List<MulticastEPGSource> MulticastEPGSources;
        #endregion

        public Settings()
        {
            HTTP_IPAdress = "0.0.0.0";
            HTTP_Port = 80;
            MulticastEPGSources = new List<MulticastEPGSource>();
        }
    }
}
