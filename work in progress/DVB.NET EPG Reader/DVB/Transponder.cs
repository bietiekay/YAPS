using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
	/// <summary>
	/// Base class for any transponder description.
	/// </summary>
    [Serializable]
	public abstract class Transponder
	{
		/// <summary>
		/// All stations in this transponder.
		/// </summary>
		[XmlIgnore]
        private List<Station> m_Stations = new List<Station>();

		/// <summary>
		/// Initialize the instance.
		/// </summary>
		protected Transponder()
		{
		}

		/// <summary>
		/// Register a <see cref="Station"/> inside this transponder.
		/// </summary>
		/// <param name="station">The station to register.</param>
		internal void AddStation(Station station)
		{
			// Validate
			if (null == station) throw new ArgumentNullException("station");

			// See if it exists
			int i = m_Stations.IndexOf(station);

			// Check mode
			if (i < 0)
			{
				// Register new
				m_Stations.Add(station);
			}
			else
			{
				// Replace existing
				m_Stations[i] = station;
			}
		}

		/// <summary>
		/// Get all stations in this transponder.
		/// </summary>
		[XmlElement("Station")]
		public Station[] Stations
		{
			get
			{
				// Create
				return m_Stations.ToArray();
			}
			set
			{
				// Reset
				m_Stations.Clear();

				// Fill
				if (null != value) m_Stations.AddRange(value);
			}
		}
	}
}
