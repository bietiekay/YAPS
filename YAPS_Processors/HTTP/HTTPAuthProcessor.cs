using System;
using System.Net;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// This Processor implements simple IP Filtering and Authentication (HTTP_AUTH)
    /// </summary>
    [Serializable]
    public static class HTTPAuthProcessor
    {
        public static List<AuthentificationUser> KnownClients = new List<AuthentificationUser>();

        #region Client Management
        public static AuthentificationUser AddUser(String Username_)
        {
            AuthentificationUser authEntry = new AuthentificationUser();

            authEntry.Username = Username_;

            KnownClients.Add(authEntry);

            return authEntry;
        }
        #endregion

        #region FindUser
        public static String IPtoUsername(String IPAdress)
        {
            foreach (AuthentificationUser User in KnownClients)
            {
                foreach (AuthentificationEntry Entry in User.AuthEntry)
                {
                    if (Entry.accessingIP == IPAdress) return User.Username;
                }
            }
            return IPAdress;
        }
        #endregion

        #region Holding Time
        public static Int32 GetAccordingHoldingTime(String IPAdress)
        {
            foreach (AuthentificationUser User in KnownClients)
            {
                foreach (AuthentificationEntry Entry in User.AuthEntry)
                {
                    if (Entry.accessingIP == IPAdress) return User.RecordingsHoldingTime;
                }
            }
            return 1; // hold one day 
        }
        #endregion

        #region Capability Checking
        #region CanAccessLiveStream
        public static bool AllowedToAccessLiveStream(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;
                        if (allowedClient.canAccessLiveStream)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to access the live streams");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region CanAccessTuxbox
        public static bool AllowedToAccessTuxbox(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;
                        if (allowedClient.canAccessTuxBox)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to access the tuxbox functionality");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region CanAccessRecordings
        public static bool AllowedToAccessRecordings(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;
                        if (allowedClient.canAccessRecordings)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to access the recordings");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region canAccessThisServer
        public static bool AllowedToAccessThisServer(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;
                        if (allowedClient.canAccessThisServer)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to access this server");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region CanCreateRecordings
        public static bool AllowedToCreateRecordings(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;
                        if (allowedClient.canCreateRecordings)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to create recordings.");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region CanDeleteRecordings
        public static bool AllowedToDeleteRecordings(IPAddress accessingIP, String createdBy)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator) return true;

                        if (allowedUser.Username == createdBy)
                        {
                            if (allowedClient.canDeleteHisOwnRecordings) return true;
                        }

                        if (allowedClient.canDeleteAllRecordings)
                        {
                            return true;
                        }
                        else
                        {
                            ConsoleOutputLogger.WriteLine("HTTPAuthProcessor: " + accessingIP.ToString() + " is not allowed to delete recordings");
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #region isAdministrator
        public static bool isAdministrator(IPAddress accessingIP)
        {
            foreach (AuthentificationUser allowedUser in KnownClients)
            {
                foreach (AuthentificationEntry allowedClient in allowedUser.AuthEntry)
                {
                    if (accessingIP.ToString() == allowedClient.accessingIP.ToString())
                    {
                        if (allowedClient.isAdministrator)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
        }
        #endregion
        #endregion
    }
}
