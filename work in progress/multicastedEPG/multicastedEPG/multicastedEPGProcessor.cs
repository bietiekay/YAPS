using System;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using JMS.DVB.EPG;

namespace YAPS
{
    #region EPG Processor
    public class multicastedEPGProcessor
    {
        #region Data and Constructor
        public List<MulticastEPGSource> MulticastEPGSources;

        private List<epgThreadInstance> ThreadInstances;

        public List<EPG_Event_Entry> CurrentlyRunningEvents;

        Hashtable EPG_Events = new Hashtable();

        bool everything_is_setup;
        bool done;

        public multicastedEPGProcessor()
        {
            everything_is_setup = false;
            done = false;
            CurrentlyRunningEvents = new List<EPG_Event_Entry>();
        }
        #endregion

        #region Flags and stuff
        /// <summary>
        /// when this is called the Scheduler Thread will shut down the next turnaround
        /// </summary>
        public void youreDone()
        {
            done = true;
        }

        /// <summary>
        /// when this is called the Scheduler Thread will shut down the next turnaround
        /// </summary>
        public void setupDone()
        {
            everything_is_setup = true;
        }
        #endregion 

        #region EPGProcessor
        public void EPGProcessor()
        {
            // wait for the settings to be loaded...
            Console.WriteLine("EPG Processor is waiting for the Settings to be loaded...");
            while (!everything_is_setup)
            {
                Thread.Sleep(10);
            }

            Console.WriteLine("EPG Processor up and running...");

            // if there are EPG Data Sources start the data receiving threads...
            ThreadInstances = new List<epgThreadInstance>();

            foreach (MulticastEPGSource source in MulticastEPGSources)
            {
                epgThreadInstance newThread = new epgThreadInstance(source, EPG_Events);

                ThreadInstances.Add(newThread);

                newThread.EPG_Thread.Start();
            }

            // wait until we're done and collect garbage once every while
            while ((!done))
            {
                Thread.Sleep(60000);
                CollectGarbageAndUpdateCurrentlyRunningList();
            }

            // we're done; shutdown
            foreach (epgThreadInstance instance in ThreadInstances)
            {
                // inform every thread to shut down...
                instance.EPG_Thread_Class.youreDone();
            }
        }
        #endregion

        #region Garbage Collector and Currently Running List Maintenance
        public void CollectGarbageAndUpdateCurrentlyRunningList()
        {
            try
            {
                DateTime now = DateTime.Now;
                List<String> DeleteThisElements = new List<string>();

                // thread safe locking of the data
                lock (EPG_Events.SyncRoot)
                {
                    foreach (EPG_Event_Entry Entry in EPG_Events.Values)
                    {
                        if (Entry.EndTime.ToLocalTime().Ticks < now.Ticks)
                        {
                            Console.WriteLine("Removing Event " + Entry.ShortDescription.Name + " on Channel " + ServiceIDtoStationNameMapper.MapService2StationName(Entry.Service) + " - " + Entry.StartTime.ToLocalTime().ToString());
                            DeleteThisElements.Add(string.Format("{0}-{1}", Entry.Service, Entry.EventIdentifier));
                        } else
                            if (Entry.StartTime.ToLocalTime().Ticks < now.Ticks)
                            {
                                // it's currently running
                                lock (CurrentlyRunningEvents)
                                {
                                    CurrentlyRunningEvents.Clear();
                                    CurrentlyRunningEvents.Add(Entry);
                                }
                            }
                    }
                    if (DeleteThisElements.Count > 0)
                    {
                        for (int i = 0; i < DeleteThisElements.Count; i++)
                        {
                            EPG_Events.Remove(DeleteThisElements[i]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("EPG_Garbage_Collection_Thread Error: " + e.Message);
                return;
            }
        }

        #endregion
    }
    #endregion

    #region EPG Thread
    public class EPGThread
    {
        private const int epgPacketSize = 188;

        private JMS.DVB.EPG.Parser EPGParser = null;
        private MulticastEPGSource EPGDataSource;
        Hashtable EPG_Events;

        bool done;

        #region JMS global stuff
        private bool m_Reading = false;
        private int m_Counter = -1;
        private int m_BytesLeft = 0;
        private List<byte[]> m_Parts = new List<byte[]>();
        #endregion

        #region Constructor n stuff
        public EPGThread(MulticastEPGSource epg_datasource, Hashtable EPGEvents)
        {
            done = false;
            EPG_Events = EPGEvents;
            EPGParser = new Parser();

            EPGParser.SectionFound += new JMS.DVB.EPG.Parser.SectionFoundHandler(SectionFound);

            EPGDataSource = epg_datasource;
        }

        /// <summary>
        /// when this is called the thread will shut down the next turnaround
        /// </summary>
        public void youreDone()
        {
            done = true;
        }
        #endregion

        public void EPG_Thread()
        {
            try
            {
                Socket MulticastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, EPGDataSource.Portnumber);
                IPAddress ip = IPAddress.Parse(EPGDataSource.IPAddress);

                MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
                MulticastSocket.Bind(ipep);
                MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

                #region we need some memory to store data withing
                byte[] receiveBuffer = new byte[2048];
                int receivedBufferLength = 0;
                #endregion

                // start the multicast client
                Console.WriteLine("EPG Thread for " + EPGDataSource.EPGName + " started.");

                // wait until we're done
                while ((!done))
                {
                    // receive data
                    receivedBufferLength = MulticastSocket.Receive(receiveBuffer);

                    //Console.Write("+");

                    // Handleit!
                    for (int i = 0; (i + epgPacketSize) <= receivedBufferLength; i += epgPacketSize) ProcessEPGPacket(receiveBuffer, i);

                    // waiting FTW!!
                    Thread.Sleep(1);
                }

                Console.WriteLine("EPG Thread for " + EPGDataSource.EPGName + " exiting.");

                // close the socket
                MulticastSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("EPG_Thread Error: " + e.Message);
                return;
            }
        }

        #region ProcessPacket

        #region ProcessEPGPacket
        private void ProcessEPGPacket(byte[] Buffer, int i)
        {
            // Test
            if (0x47 != Buffer[i++]) throw new ArgumentException("not a TS package");

            // Decode flag
            bool fstp = (0x40 == (0x40 & Buffer[i]));

            // Fast look
            int pidh = Buffer[i++] & 0x1f;
            int pidl = Buffer[i++];
            int pid = pidl + 256 * pidh;

            // Skip
            if (0x12 != pid) return;

            // Decode all (slow)
            int adap = (Buffer[i] >> 4) & 0x3;
            int counter = Buffer[i++] & 0xf;

            // Not supported by VCR.NET
            if (0 == adap) throw new ArgumentException("expected adaption or payload");

            // Check mode
            if (fstp)
            {
                // Running
                m_Reading = true;
            }
            else if (!m_Reading)
            {
                // Wait for start
                return;
            }

            // EPG should not have any adaption present
            if (2 == (2 & adap)) throw new InvalidOperationException("unexpected adaption");

            // Correct very first call
            if (m_Counter < 0)
            {
                // Start it up
                m_Counter = counter;
            }
            else if (counter != m_Counter)
            {
                // Clear buffer
                m_BytesLeft = 0;
                m_Parts.Clear();

                // Must start from scratch
                if (!fstp)
                {
                    // Skip mode
                    m_Reading = false;

                    // Reset to beginning
                    m_Counter = -1;

                    // Done so far
                    return;
                }

                // Reset to where we are
                m_Counter = counter;
            }

            // Count only if payload is present
            m_Counter = (m_Counter + 1) & 0xf;

            // Get the number of payload bytes
            int payload = 184, start = i;

            // Adjust
            if (fstp)
            {
                // Start a new paket
                m_BytesLeft = 0;
                m_Parts.Clear();

                // Load pointer field
                int skip = Buffer[start++];

                // Validate
                if (--payload < skip) throw new InvalidOperationException("pointer to large");

                // Adjust
                start += skip;
                payload -= skip;
            }

            // Read or correct the length
            if (0 == m_BytesLeft)
            {
                // Validate header
                if (payload < 3) throw new InvalidOperationException("corrupted (header)");

                // Decode
                int lowLength = Buffer[start + 2], highLength = Buffer[start + 1] & 0xf;

                // Construct the overall size
                int size = lowLength + 256 * highLength;

                // Set the counter
                m_BytesLeft = 3 + size;
            }

            // Correct
            if (payload > m_BytesLeft) payload = m_BytesLeft;

            // Correct
            m_BytesLeft -= payload;

            // Duplicate
            byte[] part = new byte[payload];

            // Fill
            Array.Copy(Buffer, start, part, 0, part.Length);

            // Remember
            m_Parts.Add(part);

            // Time to send
            if (m_BytesLeft < 1) SendTable();
        }

        private void SendTable()
        {
            // Process all
            foreach (byte[] part in m_Parts) EPGParser.OnData(part);

            // Clear
            m_Parts.Clear();
        }
        #endregion
        #endregion

        #region EPGSectionHandler
        private void SectionFound(JMS.DVB.EPG.Section section)
        {
            // Check
            if ((null == section) || !section.IsValid) return;

            // Convert
            JMS.DVB.EPG.Tables.EIT epgTable = section.Table as JMS.DVB.EPG.Tables.EIT;

            // Check it
            if ((null == epgTable) || !epgTable.IsValid) return;

            // Process all events
            foreach (JMS.DVB.EPG.EventEntry entry in epgTable.Entries)
                if (JMS.DVB.EPG.EventStatus.Running == entry.Status)
                    AddEntry(epgTable.ServiceIdentifier, entry);
        }

        private void AddEntry(ushort service, JMS.DVB.EPG.EventEntry entry)
        {
            // Create a new Entry
            EPG_Event_Entry newEventEntry = new EPG_Event_Entry();

            newEventEntry.Service = service;
            newEventEntry.EndTime = entry.StartTime.ToLocalTime()+entry.Duration;
            //newEventEntry.EIT_Table = entry.Table;
            newEventEntry.EventIdentifier = entry.EventIdentifier;
            newEventEntry.FreeCA = entry.FreeCA;
            newEventEntry.StartTime = entry.StartTime.ToLocalTime();
            newEventEntry.Status = entry.Status;
            newEventEntry.EPG_Provider = EPGDataSource.EPGName;

            // Descriptors we can have
            JMS.DVB.EPG.Descriptors.ShortEvent shortEvent = null;

            // Extended events
            List<JMS.DVB.EPG.Descriptors.ExtendedEvent> exEvents = new List<JMS.DVB.EPG.Descriptors.ExtendedEvent>();

            // Check all descriptors
            foreach (JMS.DVB.EPG.Descriptor descr in entry.Descriptors)
                if (descr.IsValid)
                {
                    // Check type
                    if (null == shortEvent)
                    {
                        // Read
                        shortEvent = descr as JMS.DVB.EPG.Descriptors.ShortEvent;
                        // Done for now
                        if (null != shortEvent)
                        {
                            // add the data
                            newEventEntry.ShortDescription.Language = shortEvent.Language;
                            newEventEntry.ShortDescription.Name = shortEvent.Name;
                            newEventEntry.ShortDescription.Text = shortEvent.Text;
                            newEventEntry.ShortDescription.Tag = shortEvent.Tag;

                            continue;
                        }
                    }

                    // Test
                    JMS.DVB.EPG.Descriptors.ExtendedEvent exEvent = descr as JMS.DVB.EPG.Descriptors.ExtendedEvent;

                    // Register
                    if (null != exEvent)
                    {
                        EPG_ExtendedEvent extevent = new EPG_ExtendedEvent();

                        extevent.DescriptorNumber = exEvent.DescriptorNumber;
                        extevent.Language = exEvent.Language;
                        extevent.LastDescriptorNumber = exEvent.LastDescriptorNumber;
                        extevent.Name = exEvent.Name;
                        extevent.Tag = exEvent.Tag;
                        extevent.Text = exEvent.Text;

                        newEventEntry.Extended.Add(extevent);
                    }
                }

            // first check if the event is currently running or in the future
            if (newEventEntry.EndTime.Ticks > DateTime.Now.Ticks)
            {
                lock (EPG_Events.SyncRoot)
                {
                    // add and check if it's already in
                    try
                    {
                        EPG_Events.Add(string.Format("{0}-{1}", service, entry.EventIdentifier), newEventEntry);

                        try
                        {
                            lock (ServiceIDtoStationNameMapper.Service2StationMapping.SyncRoot)
                            {
                                Station newStation = new Station();

                                newStation.Service = service;
                                newStation.StationName = service.ToString();

                                ServiceIDtoStationNameMapper.Service2StationMapping.Add(service, newStation);
                            }
                        }
                        catch (Exception)
                        {
                        }

                        Console.WriteLine("New Event " + newEventEntry.ShortDescription.Name + " on Channel " + ServiceIDtoStationNameMapper.MapService2StationName(service) + " - " + entry.StartTime.ToLocalTime().ToString());

                        {
                            #region XML
                            FileStream fs = new FileStream("blah.xml", FileMode.Create, FileAccess.Write);
                            try
                            {

                                List<EPG_Event_Entry> entries = new List<EPG_Event_Entry>(EPG_Events.Count);

                                foreach (object key in EPG_Events.Keys)
                                {
                                    entries.Add((EPG_Event_Entry)EPG_Events[key]);
                                }

                                XmlSerializer serializer = new XmlSerializer(typeof(List<EPG_Event_Entry>));
                                serializer.Serialize(fs, entries);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            finally
                            {
                                fs.Close();
                            }
                            #endregion
                            #region XML
                            fs = new FileStream("StationID.xml", FileMode.Create, FileAccess.Write);
                            try
                            {
                                List<Station> entries = new List<Station>(ServiceIDtoStationNameMapper.Service2StationMapping.Count);

                                foreach (object key in ServiceIDtoStationNameMapper.Service2StationMapping.Keys)
                                {
                                    entries.Add((Station)ServiceIDtoStationNameMapper.Service2StationMapping[key]);
                                }

                                XmlSerializer serializer = new XmlSerializer(typeof(List<Station>));
                                serializer.Serialize(fs, entries);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                            finally
                            {
                                fs.Close();
                            }
                            #endregion
                        }

                    }
                    catch (Exception)
                    {
                        //Console.Write("-");
                        return;
                    }
                }
            }
        }
        #endregion
    }
    #endregion

    #region StationNameMapping
    public static class ServiceIDtoStationNameMapper
    {
        public static Hashtable Service2StationMapping = new Hashtable();

        public static String MapService2StationName(ushort ServiceID)
        {
            lock (Service2StationMapping)
            {
                if (Service2StationMapping.ContainsKey(ServiceID))
                {
                    Station tstation = (Station)Service2StationMapping[ServiceID];
                    return tstation.StationName;
                }
                else
                    return ServiceID.ToString();
            }
        }
    }
    #endregion
}
