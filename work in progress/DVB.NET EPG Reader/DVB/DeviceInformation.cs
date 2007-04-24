using System;
using System.IO;
using System.Xml;
using System.Collections;

namespace JMS.DVB
{
	public class DeviceInformation
	{
		private XmlElement m_Root;

		public DeviceInformation(XmlElement provider)
		{
			// Load
			m_Root = provider;

			// Verifiy
			if ( !m_Root.Name.Equals("DVBNETProvider") ) throw new ArgumentException("bad provider definition", "file");
		}

		private XmlElement FindElement(string name)
		{
			// Report
			return (XmlElement)m_Root.SelectSingleNode(name);
		}

		public XmlNodeList Parameters
		{
			get
			{
				// Report
				return FindElement("Parameters").ChildNodes;
			}
		}

		public string UniqueIdentifier
		{
			get
			{
				// Report
				return (string)m_Root.GetAttribute("id");
			}
		}

		public override string ToString()
		{
			// Report
			return UniqueIdentifier;
		}

		internal IDeviceProvider Create(Hashtable settings)
		{
			// Clone the hashtable
			Hashtable mySettings = new Hashtable(settings);

			// See if there are parameters
			XmlElement parameters = FindElement("Parameters");

			// Process all extra settings
			if ( null != parameters )
				foreach ( XmlElement param in parameters )
					if (!mySettings.ContainsKey(param.Name))
						mySettings[param.Name] = param.InnerText;

			// Load the type name
			string typeName = FindElement("Driver").InnerText;

			// Create it
			return (IDeviceProvider)Activator.CreateInstance(Type.GetType(typeName), new object[] { mySettings });
		}

		public string[] Names
		{
			get
			{
				// Helper
				ArrayList names = new ArrayList();

				// All my names
				foreach ( XmlNode name in m_Root.SelectNodes("CardNames/CardName") )
				{
					// Remember
					names.Add(name.InnerText);
				}

				// Report
				return (string[])names.ToArray(typeof(string));
			}
		}
	}
}
