using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    [Serializable]
    public class AuthentificationUser
    {
        public String Username;

        public List<AuthentificationEntry> AuthEntry;

        public AuthentificationUser()
        {
            AuthEntry = new List<AuthentificationEntry>();
        }
    }

    [Serializable]
    public class AuthentificationEntry
    {
        public String accessingIP;

        public bool canAccessLiveStream;
        public bool canAccessRecordings;
        public bool canAccessThisServer;
        public bool canAccessTuxBox;
        public bool canCreateRecordings;
        public bool canDeleteAllRecordings;
        public bool canDeleteHisOwnRecordings;
        public bool isAdministrator;

        public AuthentificationEntry()
        {
            //accessingIP = ClientIP;
            canAccessLiveStream = false;
            canAccessRecordings = false;
            canAccessTuxBox = false;
            canAccessThisServer = false;
            canCreateRecordings = false;
            canDeleteAllRecordings = false;
            canDeleteHisOwnRecordings = true;
            isAdministrator = false;
        }
    }

}
