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
        public Int32 RecordingsHoldingTime; // how long will the recording be saved...0 is indefinite
        
        public List<AuthentificationEntry> AuthEntry;

        public AuthentificationUser()
        {
            AuthEntry = new List<AuthentificationEntry>();
            RecordingsHoldingTime = 0;
        }
    }

    [Serializable]
    public class AuthentificationEntry
    {
        public String accessingIP;

        public bool canAccessLiveStream;
        public bool canAccessRecordings;
        public bool canAccessHisRecordings;
        public bool canAccessOthersRecordings;
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
            canAccessOthersRecordings = false;
            canAccessTuxBox = false;
            canAccessThisServer = false;
            canCreateRecordings = false;
            canDeleteAllRecordings = false;
            canDeleteHisOwnRecordings = true;
            isAdministrator = false;
        }
    }

}
