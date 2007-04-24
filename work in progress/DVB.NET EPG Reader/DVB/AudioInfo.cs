using System;
using System.Text;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class AudioInfo
	{
		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// 
		/// </summary>
		[XmlAttribute]
		public ushort PID;

		/// <summary>
		/// 
		/// </summary>
		public AudioInfo()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="pid"></param>
		/// <param name="name"></param>
		public AudioInfo(ushort pid, string name)
		{
			// Remember
			Name = name;
			PID = pid;
		}
	}
}
