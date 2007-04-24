using System;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace JMS.DVB
{
	/// <summary>
	/// The device selector is used to dynamically create a DVB.NET hardware
	/// abstraction.
	/// </summary>
	/// <remarks>
	/// It will locate the <i>DVBNETProviders.xml</i> and find the active
	/// provide.
	/// </remarks>
	public class DeviceSelector: IDeviceProviderFactory
	{
		/// <summary>
		/// Helper instance for locating and parsing the <i>DVBNETProvides.xml</i>.
		/// </summary>
		private DeviceInformations m_Devices;

		/// <summary>
		/// Create a new device selector instance.
		/// </summary>
		/// <param name="settings">The <i>Path</i> entry can be used to
		/// change the location of the <i>DVBNETProviders.xml</i> relative
		/// to the path of the current executable.</param>
		public DeviceSelector(Hashtable settings)
		{
			// Create
			m_Devices = new DeviceInformations(settings);
		}

		#region IDeviceProviderFactory Members

		/// <summary>
		/// Create a new dynamic DVB.NET hardware abstraction instance according
		/// to the <i>DVBNETProviders.xml</i> settings.
		/// </summary>
		/// <returns>A new instance or <i>null</i> if none has been selected.</returns>
		/// <exception cref="ProviderMissingException">If the configuration file is
		/// missing or contains no valid configuration entries.</exception>
		public IDeviceProvider Create()
		{
			// Check
			if ( m_Devices.Count < 1 ) throw new ProviderMissingException();

			// Done
			if ( !Tools.ForceSelection ) return m_Devices.Create(m_Devices.ActiveProvider);

			// Run dialog
			Tools.RunDVBApplication("JMS.DVB.ProviderChooser, JMS.DVB", m_Devices);

			// Terminate program
			return null;
		}

		#endregion
	}
}
