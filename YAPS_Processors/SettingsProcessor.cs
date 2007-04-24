using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public class SettingsProcessor
    {
        private String RecordingsFilename;
        private String DoneRecordingsFilename;
        private String CategoriesFilename;
        private String MiscSettingsFilename;
        private String AuthentificationSettingsFilename;
        private String StationIDFilename;
        private VCRScheduler internal_vcr_scheduler;
        private HttpServer internal_http_server;

        // TODO: redo the Variable names...
        /// <summary>
        /// Creates the Settings Processor
        /// </summary>
        /// <param name="RFilename">The Recording-Settings-Path+Filename</param>
        /// <param name="DRFilename">The doneRecording-Settings-Path+Filename</param>
        /// <param name="CFilename">The Categories-Settings-Path+Filename</param>
        /// <param name="CMFilename">The ChannelNameMapping-Settings-Path+Filename</param>
        /// <param name="CPFilename">The ChannelPictureMapping-Settings-Path+Filename</param>
        /// <param name="SeFilename">The Misc-Settings-Path+Filename</param>
        /// <param name="CMAFilename">The ChannelMulticastAdressFilename+Filename</param>
        /// <param name="vcrScheduler">object of the vcrScheduler</param>
        /// <param name="webserver">object of the webserver</param>
        public SettingsProcessor(String RFilename, String DRFilename,String CFilename, String SeFilename, String AuSFilename, String StationAndChannels, VCRScheduler vcrScheduler, HttpServer webserver)
        {
            RecordingsFilename = RFilename;
            DoneRecordingsFilename = DRFilename;
            CategoriesFilename = CFilename;
            MiscSettingsFilename = SeFilename;
            StationIDFilename = StationAndChannels;
            internal_http_server = webserver;
            AuthentificationSettingsFilename = AuSFilename;
            internal_vcr_scheduler = vcrScheduler;
        }

        #region Hashtable Entry Serialization/Deserialization
        public class Entry
        {
            public object Key;
            public object Value;

            public Entry()
            {
            }

            public Entry(object key, object value)
            {
                Key = key;
                Value = value;
            }
        }
        #endregion

        #region SaveSettings
        /// <summary>
        /// this method saves the settings to the actual config file
        /// </summary>
        public void SaveSettings()
        {
            lock (this)
            {
                FileStream fs;
                // TODO: make it thread safe

                ConsoleOutputLogger.WriteLine("Saving Settings...");

                #region Categories

                #region XML
                fs = new FileStream(CategoriesFilename+".xml", FileMode.Create, FileAccess.Write);
                try
                {
                    System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(internal_vcr_scheduler.Category_Processor.Categories.GetType());
                    xmls.Serialize(fs, internal_vcr_scheduler.Category_Processor.Categories);
                }
                finally
                {
                    fs.Close();
                }
                #endregion

                #endregion

                #region Recordings

                #region XML
                fs = new FileStream(RecordingsFilename + ".xml", FileMode.Create, FileAccess.Write);
                try
                {

                    List<Recording> entries = new List<Recording>(internal_vcr_scheduler.Recordings.Count);

                    foreach (object key in internal_vcr_scheduler.Recordings.Keys)
                    {
                        entries.Add((Recording)internal_vcr_scheduler.Recordings[key]);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(List<Recording>));
                    serializer.Serialize(fs, entries);
                }
                finally
                {
                    fs.Close();
                }
                #endregion

                #endregion

                #region Station and Channel Mapping
                #region XML
                fs = new FileStream(StationIDFilename + ".xml", FileMode.Create, FileAccess.Write);
                try
                {
                    System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(ChannelAndStationMapper.ChannelsAndStations.GetType());
                    xmls.Serialize(fs, ChannelAndStationMapper.ChannelsAndStations);
                }
                finally
                {
                    fs.Close();
                }
                #endregion
                #endregion

                #region DoneRecordings

                #region XML
                fs = new FileStream(DoneRecordingsFilename + ".xml", FileMode.Create, FileAccess.Write);
                try
                {

                    List<Recording> entries = new List<Recording>(internal_vcr_scheduler.doneRecordings.Count);

                    foreach (object key in internal_vcr_scheduler.doneRecordings.Keys)
                    {
                        entries.Add((Recording)internal_vcr_scheduler.doneRecordings[key]);
                    }

                    XmlSerializer serializer = new XmlSerializer(typeof(List<Recording>));
                    serializer.Serialize(fs, entries);
                }
                finally
                {
                    fs.Close();
                }
                #endregion
                #endregion

                #region Misc Settings

                #region XML
                fs = new FileStream(MiscSettingsFilename + ".xml", FileMode.Create, FileAccess.Write);
                try
                {
                    System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(internal_http_server.Settings.GetType());
                    xmls.Serialize(fs, internal_http_server.Settings);
                }
                finally
                {
                    fs.Close();
                }
                #endregion

                #endregion

                #region Authentification Settings
                fs = new FileStream(AuthentificationSettingsFilename + ".xml", FileMode.Create, FileAccess.Write);
                try
                {
                    System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(HTTPAuthProcessor.KnownClients.GetType());
                    xmls.Serialize(fs, HTTPAuthProcessor.KnownClients);
                }
                finally
                {
                    fs.Close();
                }
                #endregion
            }
        }
        #endregion

        #region LoadSettings
        /// <summary>
        /// guess what it does...it Loads the Settings from the Config File; it's usually only called at startup time
        /// </summary>
        public void LoadSettingsXML()
        {
            // TODO: Error checking; what happens when the config files are corrupt?
            lock (this)
            {
                FileStream fs;

                ConsoleOutputLogger.WriteLine("Loading Settings...");

                #region Categories
                if (File.Exists(CategoriesFilename + ".xml"))
                {
                    fs = new FileStream(CategoriesFilename+".xml", FileMode.Open, FileAccess.Read);

                    try
                    {
                        internal_vcr_scheduler.Category_Processor.Categories.Clear();
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Category>));
                        internal_vcr_scheduler.Category_Processor.Categories = (List<Category>)serializer.Deserialize(fs);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                else
                    ConsoleOutputLogger.WriteLine("No Categories configfile found.");
                #endregion

                #region Stations and Channels and Multicast
                if (File.Exists(StationIDFilename+".xml"))
                {
                    fs = new FileStream(StationIDFilename+".xml", FileMode.Open, FileAccess.Read);
                    try
                    {
                        ChannelAndStationMapper.ChannelsAndStations.Clear();
                        XmlSerializer serializer = new XmlSerializer(typeof(List<StationAndChannel>));
                        ChannelAndStationMapper.ChannelsAndStations = (List<StationAndChannel>)serializer.Deserialize(fs);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                #endregion

                #region Recordings
                if (File.Exists(RecordingsFilename + ".xml"))
                {
                    fs = new FileStream(RecordingsFilename + ".xml", FileMode.Open, FileAccess.Read);
                    try
                    {
                     //   BinaryFormatter bf = new BinaryFormatter();
                     //   internal_vcr_scheduler.Recordings = (Hashtable)bf.Deserialize(fs);

                        Hashtable tht = new Hashtable();
                        internal_vcr_scheduler.Recordings.Clear();
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Recording>));
                        List<Recording> list = (List<Recording>)serializer.Deserialize(fs);

                        foreach (Recording entry in list)
                        {
                            internal_vcr_scheduler.Recordings.Add(entry.Recording_ID, entry);
                        }
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                else
                    ConsoleOutputLogger.WriteLine("No Recordings configfile found.");
                #endregion

                #region DoneRecordings
                if (File.Exists(DoneRecordingsFilename + ".xml"))
                {
                    fs = new FileStream(DoneRecordingsFilename + ".xml", FileMode.Open, FileAccess.Read);
                    try
                    {
                        //BinaryFormatter bf = new BinaryFormatter();
                        //internal_vcr_scheduler.doneRecordings = (Hashtable)bf.Deserialize(fs);

                        Hashtable tht = new Hashtable();

                        internal_vcr_scheduler.doneRecordings.Clear();
                        XmlSerializer serializer = new XmlSerializer(typeof(List<Recording>));
                        List<Recording> list = (List<Recording>)serializer.Deserialize(fs);

                        foreach (Recording entry in list)
                        {
                            internal_vcr_scheduler.doneRecordings.Add(entry.Recording_ID, entry);
                        }

                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                else
                    ConsoleOutputLogger.WriteLine("No DoneRecordings configfile found.");
                #endregion

                #region MiscSettings
                if (File.Exists(MiscSettingsFilename + ".xml"))
                {
                    fs = new FileStream(MiscSettingsFilename + ".xml", FileMode.Open, FileAccess.Read);
                    try
                    {
                        Settings tSettings;

                        //BinaryFormatter bf = new BinaryFormatter();
                        XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                        tSettings = (Settings)serializer.Deserialize(fs);

                        internal_http_server.Settings = tSettings;
                        internal_vcr_scheduler.Settings = tSettings;
                    }
                    finally
                    {
                        fs.Close();
                        CheckMiscSettings(internal_http_server.Settings);
                    }
                }
                else
                    ConsoleOutputLogger.WriteLine("No Misc-Settings configfile found.");
                #endregion

                #region Authentification Settings
                if (File.Exists(AuthentificationSettingsFilename + ".xml"))
                {
                    fs = new FileStream(AuthentificationSettingsFilename + ".xml", FileMode.Open, FileAccess.Read);
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(List<AuthentificationUser>));
                        HTTPAuthProcessor.KnownClients = (List<AuthentificationUser>)serializer.Deserialize(fs);
                    }
                    finally
                    {
                        fs.Close();
                    }
                }
                else
                    ConsoleOutputLogger.WriteLine("No Authentification-Settings configfile found.");

                #endregion
            }
        }
        #endregion

        #region Setting Checks
        /// <summary>
        /// this one checks the misc settings for plausability 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>a String with the error/warning messages</returns>
        public String CheckMiscSettings(Settings settings)
        {
            StringBuilder CheckOutput = new StringBuilder();
            bool foundCriticalError = false;

            // check if the ip adress is configured...
            #region CriticalErrors
            if (settings.HTTP_IPAdress == "0.0.0.0")
            {
                foundCriticalError = true;
                CheckOutput.AppendLine("LoadSetting.MiscSettings Error: You HAVE TO set a valid IP Adress that YAPS is listening on! Do that by editing the YAPS.Settings.dat.xml file!");
            }
            #endregion


            #region React
            if (foundCriticalError)
            {
                Exception ex = new Exception(CheckOutput.ToString());
                throw ex;
            }
            else
            {
                ConsoleOutputLogger.WriteLine(CheckOutput.ToString());
            }
            #endregion

            return CheckOutput.ToString();
        }
        #endregion
    }
}