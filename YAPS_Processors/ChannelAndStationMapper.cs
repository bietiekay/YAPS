using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// Implements the ChannelAndStationID Mapper that holds all the information about Channels and EPG Services/Stations
    /// </summary>
    public static class ChannelAndStationMapper
    {
        #region Data
        // holds all the data
        public static List<StationAndChannel> ChannelsAndStations = new List<StationAndChannel>();
        #endregion

        #region Add
        public static bool Add(Int32 ChannelNumber, String ChannelName, String ChannelPictureURL, ushort ServiceID, String MulticastIP, String MulticastPort)
        {
            try
            {
                // first check if it already exists...
                foreach (StationAndChannel station_ in ChannelsAndStations)
                {
                    // exists, now exit
                    if (station_.ServiceID == ServiceID)
                        return false;
                }

                // does not exist, so we add it...
                StationAndChannel newData = new StationAndChannel();

                newData.ChannelName = ChannelName;
                newData.ChannelPictureURL = ChannelPictureURL;
                newData.ChannelNumber = ChannelNumber;
                newData.ServiceID = ServiceID;
                newData.MulticastIP = MulticastIP;
                newData.MulticastPort = MulticastPort;

                lock (ChannelsAndStations)
                {
                    ChannelsAndStations.Add(newData);
                }

                return true;
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("ChannelAndStationMapper.Add: " + e.Message);
                return false;
            }
        }
        #endregion

        #region Mapping
        public static StationAndChannel Number2Data(int ChannelNumber)
        {
            lock (ChannelsAndStations)
            {
                foreach (StationAndChannel _station in ChannelsAndStations)
                {
                    if (_station.ChannelNumber == ChannelNumber)
                        return _station;
                }
            }
            return null;
        }
        public static StationAndChannel Name2Data(String ChannelName)
        {
            lock (ChannelsAndStations)
            {
                foreach (StationAndChannel _station in ChannelsAndStations)
                {
                    if (_station.ChannelName.ToLower() == ChannelName.ToLower())
                        return _station;
                }
            }
            return null;
        }

        public static String Number2Name(int ChannelNumber)
        {
            try
            {
                return Number2Data(ChannelNumber).ChannelName;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Int32 Name2Number(String ChannelName)
        {
            try
            {
                return Name2Data(ChannelName).ChannelNumber;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public static String Name2Picture(String ChannelName)
        {
            String Picture = Name2Data(ChannelName).ChannelPictureURL;

            if (Picture == null)
                return "";
            else
                return Picture;
        }
        public static String Number2Picture(int ChannelNumber)
        {
            // yes I know... but well 
            // TODO: make it prettier
            return Name2Picture(Number2Data(ChannelNumber).ChannelName);
        }

        public static bool HasPicture(int ChannelNumber)
        {
            if (Number2Picture(ChannelNumber) == "")
                return false;
            else
                return true;
        }

        public static String ServiceID2Name(ushort ServiceID)
        {
            lock (ChannelsAndStations)
            {
                foreach (StationAndChannel _station in ChannelsAndStations)
                {
                    if (_station.ServiceID == ServiceID)
                        return _station.ChannelName;
                }
                return "";
            }
        }
        #endregion
    }
}
