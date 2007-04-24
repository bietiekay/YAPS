using System;
using JMS.DVB;
using System.Text;
using System.Collections;

namespace JMS.DVB.EPG
{
	/// <summary>
	/// This class is attached to the EPG and locates all service channels
	/// related to a given channel.
	/// </summary>
	public class ServiceParser
	{
		/// <summary>
		/// Currently link descriptors use Windows encoding.
		/// </summary>
		/// <remarks>
		/// At least the german PayTV station PREMIERE.
		/// </remarks>
		private static Encoding CodePage = Encoding.GetEncoding(1252);

		/// <summary>
		/// All services found related to the current station.
		/// </summary>
		private Hashtable m_ServiceNames = new Hashtable();

		/// <summary>
		/// Helper instance for EPG parsing.
		/// </summary>
		private Parser EPGParser = null;

		/// <summary>
		/// Related hardware device.
		/// </summary>
		//private IDeviceProvider DVBDevice;

		/// <summary>
		/// Synchronize access to currently shown station.
		/// </summary>
		private object m_SyncStation = new object();

		/// <summary>
		/// The currently show station.
		/// </summary>
		//private Station Portal;

		/// <summary>
		/// Create a new instance and start EPG parsing on PID <i>0x12</i>.
		/// </summary>
		/// <param name="device">Related hardware device.</param>
		/// <param name="portal">The currently show station.</param>
		/*public ServiceParser(IDeviceProvider device, Station portal)
		{
			// Remember
			DVBDevice = device;
			Portal = portal;

			// Create EPG parser
			EPGParser = new Parser(DVBDevice);

			// Attach handler
			EPGParser.SectionFound += new Parser.SectionFoundHandler(EPGSectionFound);
		}*/

		/// <summary>
		/// Get the current station.
		/// </summary>
		/*private Station CurrentPortal
		{
			get
			{
				// Report
				lock (m_SyncStation) return Portal;
			}
		}*/

		/// <summary>
		/// Change the active station.
		/// </summary>
		/// <remarks>
		/// This allows the client to keep the EPG filter installed
		/// when changing stations.
		/// </remarks>
		/// <param name="station">The new station to focus upon.</param>
		/*public void ChangeStation(Station station)
		{
			// Update
			lock (m_SyncStation) Portal = station;

			// Clear
			lock (m_ServiceNames) m_ServiceNames.Clear();
		}*/

		/// <summary>
		/// Parse some EPG information and try to extract the data of the current
		/// service group.
		/// </summary>
		/// <param name="section">Currently parsed SI table.</param>
		private void EPGSectionFound(Section section)
		{
			// Test
			Tables.EIT eit = section.Table as Tables.EIT;

			// Not us
			if ( null == eit ) return;

			// Process all events
			foreach ( EventEntry evt in eit.Entries )
			{
				// What to add
				ArrayList ids = new ArrayList(), names = new ArrayList();

				// Make sure that this is us
				bool found = false;

				// Run over
				foreach ( Descriptor descr in evt.Descriptors )
				{
					// Check type
					Descriptors.Linkage info = descr as Descriptors.Linkage;
					if ( null == info ) continue;

					// Check type (PREMIERE)
					if ( 176 != info.LinkType ) continue;

					// Create identifier
					Identifier id = new Identifier(info.OriginalNetworkIdentifier, info.TransportStreamIdentifier, info.ServiceIdentifier);

					// Try to locate the related station
					/*Station real = DVBDevice.FindStation(id);
					if ( null == real )
					{
						// Could be service channel
						id = new Identifier(info.ServiceIdentifier, 0xffff, info.ServiceIdentifier);
						real = DVBDevice.FindStation(id);

						// Try again
						if ( null == real ) continue;
					}*/

					// Check the first one
					//if ( !found ) found = Equals(id, CurrentPortal);

					// Remember
					names.Add(string.Format("{0},{1}", names.Count, CodePage.GetString(info.PrivateData)));
					ids.Add(id);
				}

				// Register
				if ( found )
					lock (m_ServiceNames)
						for ( int i = ids.Count ; i-- > 0 ; )
							m_ServiceNames[ids[i]] = names[i];
			}
		}

		/// <summary>
		/// Report the list of services found in the current group.
		/// </summary>
		public Hashtable ServiceMap
		{
			get
			{
				// Create new
				Hashtable map = new Hashtable();

				// Synchronize
				lock (m_ServiceNames) 
					foreach ( DictionaryEntry ent in m_ServiceNames ) 
						map[ent.Key] = ent.Value;

				// Report
				return map;
			}
		}
	}
}
