using System;
using System.IO;
using System.Xml;
using Microsoft.Win32;
using System.Threading;
using System.Collections;
using System.Windows.Forms;

namespace JMS.DVB
{
	public class DeviceInformations : IEnumerable
	{
		private const string PathKey = "Path";

		public readonly Hashtable Settings;

		private XmlDocument m_File = new XmlDocument();
		private Hashtable m_Devices = new Hashtable();

		public DeviceInformations()
			: this(new Hashtable())
		{
		}

		public DeviceInformations(Hashtable settings)
		{
			// Remember
			Settings = settings;

			// Attach to the current application
			FileInfo exe = new FileInfo(Application.ExecutablePath);

			// Get the root
			string root = exe.DirectoryName;

			// Check for override
			if (Settings.ContainsKey(PathKey))
			{
				// Merge
				root = Path.Combine(root, (string)Settings[PathKey]);

				// Remove
				Settings.Remove(PathKey);
			}

			// Attach to the provider configuration file
			FileInfo path = new FileInfo(Path.Combine(root, "DVBNetProviders.xml"));

			// Process
			if (path.Exists)
			{
				// Load the DOM from file
				m_File.Load(path.FullName);
			}
			else
			{
				// Me
				Type me = GetType();

				// Load the DOM from resource
				using (Stream providers = me.Assembly.GetManifestResourceStream(me.Namespace + ".DVBNETProviders.xml"))
				{
					// Load
					m_File.Load(providers);
				}
			}

			// Verify
			if (!m_File.DocumentElement.Name.Equals("DVBNETProviders")) throw new ArgumentException("bad provider definition", "file");
			if (!Equals(m_File.DocumentElement.GetAttribute("SchemaVersion"), "2.6")) throw new ArgumentException("invalid schema version", "file");

			// All providers
			foreach (XmlElement provider in m_File.DocumentElement.SelectNodes("DVBNETProvider"))
			{
				// Create new
				DeviceInformation info = new DeviceInformation(provider);

				// Register
				m_Devices[info.UniqueIdentifier] = info;
			}
		}

		public int Count
		{
			get
			{
				// Forward
				return m_Devices.Count;
			}
		}

		public IEnumerable DeviceNames
		{
			get
			{
				// Report
				return m_Devices.Keys;
			}
		}

		public DeviceInformation this[string uniqueIdentifier]
		{
			get
			{
				// Load
				DeviceInformation device = (DeviceInformation)m_Devices[uniqueIdentifier];

				// Validate
				if (null == device) throw new ArgumentException("no device " + uniqueIdentifier, "uniqueIdentifier");

				// Report
				return device;
			}
		}

		public IDeviceProvider Create(DeviceInformation provider)
		{
			// Forward
			return provider.Create(Settings);
		}

		public IDeviceProvider Create(DeviceInformation provider, Hashtable parameterOverwrites)
		{
			// Clone hashtable
			Hashtable settings = new Hashtable(Settings);

			// Merge
			foreach (DictionaryEntry parameter in parameterOverwrites)
			{
				// Merge in
				settings[parameter.Key] = parameter.Value;
			}

			// Forward
			return provider.Create(settings);
		}

		public DeviceInformation ActiveProvider
		{
			get
			{
				// Attach to registry
				using (RegistryKey key = Tools.GetConfiguration(false))
				{
					// Not found
					if (null == key) return null;

					// Load name
					string id = (string)key.GetValue("ProviderName");

					// None
					if ((null == id) || (id.Length < 1)) return null;

					// Report
					return this[id];
				}
			}
		}

		public void ChangeActive(DeviceInformation device)
		{
			// No change at all
			if (device == ActiveProvider) return;

			// Attach to registry
			using (RegistryKey key = Tools.GetConfiguration(true))
			{
				// Not found
				if (null == key) return;

				// Check
				if (null == device)
				{
					// Store
					key.SetValue("ProviderName", string.Empty);
				}
				else
				{
					// Store
					key.SetValue("ProviderName", device.UniqueIdentifier);
				}
			}
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			// Forward
			return m_Devices.Values.GetEnumerator();
		}

		#endregion
	}
}
