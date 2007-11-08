using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace YAPS
{
    /*
     * YAPS - Yet Another Proxy Server: UDP Multicast to TCP Unicast Proxy (with VCR)
     * (C) Daniel Kirstenpfad 2006-2007, HTTP portions (C) 2001 by Sam Pullara; ICON (C) 2003-2004 by Steven W. Smith
     * EPG portions (C) Dr. Jochen Manns
     * 
     * This is a small tool which allows you to proxy udp rtp multicast to tcp unicast (wihtout the rtp enclosure).
     * Beside this functionality YAPS implements a HTTP webserver and a VCR.
     * 
     * For updates and more information visit the YAPS website: http://www.technology-ninja.com
     * 
     */
    // TODO: cleanup the ChannelMapper and update it for future use (xmltv, epg, ...)
    // TODO: implement repetitive events
    // TODO: Settings Page + Exportables
    // TODO: add the possibility to set the path where the recordings are stored (maybe set more than one path)
    // TOOD: reorganize Channel data structures to allow real IP adress and port numbers
    // TODO: allow Reencoding of recordings (Download-Archive-Upload) through a client software (using mencoder)
    // TODO: add feature which auto-deletes old recordings after a specific number of recordings of that category is reached...
    // TODO: add feature which auto-deletes old recordings when we're running out of drive space
    // BUG: If the endtime of a recording is before it starts some things break...

    /// <summary>
    /// Yet Another Proxy Server: UDP Multicast to TCP Unicast Proxy and VCR
    /// </summary>
    class YAPSApp
	{
		[STAThread]
		static void Main(String[] args)
        {
            #region ConsoleOutputLogger
            ConsoleOutputLogger.verbose = true;
            ConsoleOutputLogger.writeLogfile = true;
            #endregion

            #region Logo
            ConsoleOutputLogger.WriteLine("Yet Another Proxy Server: UDP Multicast to TCP Unicast Proxy "+System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            ConsoleOutputLogger.WriteLine("(C) 2006-2007 Daniel Kirstenpfad and the YAPS Team - http://www.technology-ninja.com");
            //ConsoleOutputLogger.verbose = false;
            #endregion

            #region Data
            HttpServer httpServer;
            Cassini.Server CassiniServer;
            VCRScheduler vcrScheduler;
            XBMCSyncProcessor SyncProcessor;
			int port = 80;
			string docRoot = ".";
            httpServer = new HttpServer(docRoot, port);
            vcrScheduler = new VCRScheduler(httpServer);
            vcrScheduler.Settings = new Settings();
            httpServer.Settings = vcrScheduler.Settings;
            SyncProcessor = new XBMCSyncProcessor(httpServer);
            #endregion

            #region Load the Settings...
            SettingsProcessor Configuration = new SettingsProcessor(".\\settings\\YAPS.Recordings.dat", ".\\settings\\YAPS.DoneRecordings.dat", ".\\settings\\YAPS.Categories.dat", ".\\settings\\YAPS.Settings.dat", ".\\settings\\YAPS.Authentification.dat", ".\\settings\\YAPS.StationIDs.dat", vcrScheduler, httpServer);

            Configuration.LoadSettingsXML();
            // tell the processors that we exist...
            vcrScheduler.Configuration = Configuration;
            httpServer.Configuration = Configuration;

            Configuration.LoadSettingsXML();

            Configuration.SaveSettings();
            #endregion

            #region Testing

            //List<Recording> blah = Sorter.SortRecordingTable(vcrScheduler.doneRecordings, true);
            //ConsoleOutputLogger.WriteLine("Blah");

            // set up testing recording data
            /*Recording testRecording = new Recording();

            testRecording.Channel = "13";
            testRecording.StartsAt = DateTime.Now.AddHours(1);
            testRecording.EndsAt = testRecording.StartsAt.AddMinutes(30);

            testRecording.Recording_Filename = testRecording.Recording_ID.ToString();
            testRecording.Recording_Name = "Das perfekte Dinner";

            vcrScheduler.Recordings.Add(testRecording.Recording_ID, testRecording);
            
            testRecording = new Recording();

            testRecording.Channel = "20";
            testRecording.StartsAt = DateTime.Now.AddDays(1);
            testRecording.EndsAt = testRecording.StartsAt.AddMinutes(30);

            testRecording.Recording_Filename = testRecording.Recording_ID.ToString();
            testRecording.Recording_Name = "testaufnahme";

            vcrScheduler.Recordings.Add(testRecording.Recording_ID, testRecording);
            */
            /*
            foreach (Recording aufnahme in vcrScheduler.Recordings.Values)
            {
                aufnahme.Categories = new List<Category>();
            }
            */
            /*
            foreach (Recording recording in vcrScheduler.doneRecordings.Values)
            {
                RecordingsThumbnail.CreateRecordingsThumbnail(recording, XBMCPlaylistFilesHelper.generateThumbnailFilename(recording));
            }*/
            
            #endregion

            #region channelmappinginit
            /*
            ChannelAndStationMapper.Add(5, "ZDFtheaterkanal", "/images/channels/zdftheater.png", 10, "239.255.2.5", "505");
            ChannelAndStationMapper.Add(6, "3Sat", "/images/channels/3sat.png", 28007, "239.255.2.6", "506");
            ChannelAndStationMapper.Add(39, "KIKA", "/images/channels/kika.png", 28008, "239.255.2.39", "5039");

            ChannelAndStationMapper.Add(10, "ZDF", "/images/channels/zdf.png", 28006, "239.255.2.10", "5010");
            ChannelAndStationMapper.Add(11, "ZDFdokukanal", "/images/channels/zdfdoku.png", 28014, "239.255.2.11", "5011");
            ChannelAndStationMapper.Add(11, "ZDFinfokanal", "/images/channels/zdfinfo.png", 28011, "239.255.2.12", "5012");
            ChannelAndStationMapper.Add(13, "Sat1", "/images/channels/sat1.png", 46, "239.255.2.13", "5013");
            ChannelAndStationMapper.Add(14, "ProSieben", "/images/channels/pro7.png", 898, "239.255.2.14", "5014");
            ChannelAndStationMapper.Add(15, "Kabel1", "/images/channels/kabel1.png", 899, "239.255.2.15", "5015");
            ChannelAndStationMapper.Add(16, "DSF", "/images/channels/dsf.png", 900, "239.255.2.16", "5016");
            ChannelAndStationMapper.Add(17, "N24", "/images/channels/n24.png", 47, "239.255.2.17", "5017");
            ChannelAndStationMapper.Add(18, "RTL", "/images/channels/rtl.png", 12003, "239.255.2.18", "5018");
            ChannelAndStationMapper.Add(19, "RTL2", "/images/channels/rtl2.png", 12020, "239.255.2.19", "5019");
            ChannelAndStationMapper.Add(20, "VOX", "/images/channels/vox.png", 12060, "239.255.2.20", "5020");
            ChannelAndStationMapper.Add(21, "ntv", "/images/channels/ntv.png", 12090, "239.255.2.21", "5021");
            ChannelAndStationMapper.Add(22, "SuperRTL", "/images/channels/superrtl.png", 12040, "239.255.2.22", "5022");
            ChannelAndStationMapper.Add(23, "ARD", "/images/channels/ard.png", 28106, "239.255.2.23", "5023");
            ChannelAndStationMapper.Add(24, "arte", "/images/channels/arte.png", 1, "239.255.2.24", "5024");
            ChannelAndStationMapper.Add(25, "BR", "/images/channels/br.png", 2, "239.255.2.25", "5025");
            ChannelAndStationMapper.Add(26, "BR-Alpha", "/images/channels/br-alpha.png", 28112, "239.255.2.26", "5026");
            ChannelAndStationMapper.Add(27, "HR", "/images/channels/hr.png", 28108, "239.255.2.27", "5027");
            ChannelAndStationMapper.Add(28, "Phoenix", "/images/channels/phoenix.png", 28114, "239.255.2.28", "5028");
            ChannelAndStationMapper.Add(29, "SWR BW", "/images/channels/swr.png", 3, "239.255.2.29", "5029");
            ChannelAndStationMapper.Add(30, "WDR", "/images/channels/wdr.png", 28395, "239.255.2.30", "5030");
            ChannelAndStationMapper.Add(31, "Tele5", "/images/channels/tele5.png", 4, "239.255.2.31", "5031");
            Configuration.SaveSettings();*/
            #endregion

            #region epg multicast source init
            /*          
            MulticastEPGSource epgsource = new MulticastEPGSource();

            // I know this variant is ugly, but this way the epgsource is serializable
            epgsource.EPGName = "EPG 1";
            epgsource.IPAddress = "239.255.18.1";
            epgsource.Portnumber = 1801;

            httpServer.Settings.MulticastEPGSources.Add(epgsource);
            
            epgsource = new MulticastEPGSource();
            epgsource.EPGName = "EPG 2";
            epgsource.IPAddress = "239.255.18.2";
            epgsource.Portnumber = 1802;
            httpServer.Settings.MulticastEPGSources.Add(epgsource);

            epgsource = new MulticastEPGSource();
            epgsource.EPGName = "EPG 3";
            epgsource.IPAddress = "239.255.18.3";
            epgsource.Portnumber = 1803;
            httpServer.Settings.MulticastEPGSources.Add(epgsource);

            Configuration.SaveSettings();
            */
            #endregion

            #region Authentification
            //AuthentificationUser aUser;
            //AuthentificationEntry aEntry;

            // pre configured allowed clients
            /*
            aUser = HTTPAuthProcessor.AddUser("BtK");
            aEntry = new AuthentificationEntry();
            aEntry.accessingIP = "127.0.0.1";
            aEntry.isAdministrator = true;
            aUser.AuthEntry.Add(aEntry);
            */
            #endregion

            #region Starting up...
                #region internal HTTP Server 
                ConsoleOutputLogger.WriteLine("Starting internal HTTP Server...");
			    Thread http_server_thread = new Thread(new ThreadStart(httpServer.listen));
			    http_server_thread.Start();
                #endregion

                #region Cassini HTTP Server
                ConsoleOutputLogger.WriteLine("Starting Cassini HTTP Server...");
                CassiniServer = new Cassini.Server(httpServer.Settings.Cassini_Port, httpServer.Settings.Cassini_VirtualDirectory, httpServer.Settings.Cassini_Root_Directory);
                CassiniServer.Start();
                #endregion

                #region XBMC Sync
                ConsoleOutputLogger.WriteLine("Starting XBMC Sync Processor...");
                Thread xbmc_syncprocessor_thread = new Thread(new ThreadStart(SyncProcessor.SyncProcessor));
                xbmc_syncprocessor_thread.Start();
                #endregion

                #region VCR
                ConsoleOutputLogger.WriteLine("Starting VCR Scheduler...");
                Thread vcr_scheduler_thread = new Thread(new ThreadStart(vcrScheduler.RecordingScheduler));
                vcr_scheduler_thread.Start();
                #endregion

                #region EPG
                multicastedEPGProcessor EPG_Processor = new multicastedEPGProcessor(httpServer);

                EPG_Processor.MulticastEPGSources = httpServer.Settings.MulticastEPGSources;

                EPG_Processor.setupDone();

                Thread epg_processor_thread = new Thread(new ThreadStart(EPG_Processor.EPGProcessor));
                epg_processor_thread.Start();
                #endregion
            #endregion
            }
	}
}
