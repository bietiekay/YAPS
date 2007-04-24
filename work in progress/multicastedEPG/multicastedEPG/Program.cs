using System;
using System.Threading;
using System.IO;
using System.Xml.Serialization;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * EPG IPs:
             *          239.255.18.1:1801
             *          239.255.18.2:1802
             *          239.255.18.3:1803
             *          239.255.18.4:1804
             */
            multicastedEPGProcessor EPG_Processor = new multicastedEPGProcessor();

            #region Load StationID XML File
            if (File.Exists("StationID.xml"))
            {
                FileStream fs = new FileStream("StationID.xml", FileMode.Open, FileAccess.Read);
                try
                {
                    Hashtable tht = new Hashtable();

                    XmlSerializer serializer = new XmlSerializer(typeof(List<Station>));
                    List<Station> list = (List<Station>)serializer.Deserialize(fs);

                    foreach (Station entry in list)
                    {
                        tht[entry.Service] = entry;
                    }
                    ServiceIDtoStationNameMapper.Service2StationMapping = tht;
                }
                finally
                {
                    fs.Close();
                }
            }
            #endregion

            try
            {
                EPG_Processor.MulticastEPGSources = new List<MulticastEPGSource>();

                EPG_Processor.MulticastEPGSources.Add(new MulticastEPGSource("239.255.18.1", 1801, "EPG 1"));
                EPG_Processor.MulticastEPGSources.Add(new MulticastEPGSource("239.255.18.2", 1802, "EPG 2"));
                //EPG_Processor.MulticastEPGSources.Add(new MulticastEPGSource("239.255.18.3", 1802, "EPG 3"));
                EPG_Processor.MulticastEPGSources.Add(new MulticastEPGSource("239.255.18.4", 1804, "EPG 4"));

                EPG_Processor.setupDone();

                Thread epg_processor_thread = new Thread(new ThreadStart(EPG_Processor.EPGProcessor));
                epg_processor_thread.Start();

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Main Error: " + e.Message);
            }
            finally
            {
                EPG_Processor.youreDone();
            }
        }
    }
}
