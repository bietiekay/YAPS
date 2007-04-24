using System;
using System.Collections;
using System.Globalization;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace JMS.DVB
{
	/// <summary>
	/// Base class describing a single station.
	/// </summary>
	[Serializable]
    public class Station: Identifier
	{
		/// <summary>
		/// 
		/// </summary>
		private static Dictionary<string, string> m_LanguageMap = new Dictionary<string, string>();

		/// <summary>
		/// 
		/// </summary>
		private static Dictionary<string, string> m_LanguageMapEx = new Dictionary<string, string>();

		/// <summary>
		/// The transponder this station belongs to.
		/// </summary>
        [XmlIgnore]
        public Transponder Transponder;

		/// <summary>
		/// The name of the transponder.
		/// </summary>
        public string TransponderName;

		/// <summary>
		/// The name of this station realtive to the transponder.
		/// </summary>
		[XmlAttribute]
        public string Name;

		/// <summary>
		/// The video stream identifier.
		/// </summary>
		[XmlElement("Video")]
        public ushort VideoPID;

		/// <summary>
		/// The stream identifier of the primary audio signal.
		/// </summary>
        [XmlElement("Audio")]
        public ushort AudioPID;

		/// <summary>
		/// Stream identifier for the Dolby Digital (AC3) audio signal.
		/// </summary>
        [XmlElement("AC3")]
        public ushort AC3PID;

		/// <summary>
		/// Program clock reference stream identifier.
		/// </summary>
        [XmlElement("PCR")]
        public ushort PCRPID;

		/// <summary>
		/// Stream identifier for video text data.
		/// </summary>
        [XmlElement("TTX")]
        public ushort TTXPID;

		/// <summary>
		/// Maps audio names to the related stream identifiers.
		/// </summary>
        [XmlIgnore]
        private Hashtable m_AudioMap = new Hashtable();

		/// <summary>
		/// See if this station send encrypted.
		/// </summary>
        public bool Encrypted;

		/// <summary>
		/// The type of the video stream - 255 if not known.
		/// </summary>
		public byte VideoType = 255;

        /// <summary>
        /// Create an empty instance used for serialization.
        /// </summary>
        public Station()
        {
        }

		/// <summary>
		/// Initialize the instance.
		/// </summary>
		/// <param name="transponder">The transponder this station is bound to.</param>
		/// <param name="networkID">The original network identifier.</param>
		/// <param name="transportID">The transport strea, identifier.</param>
		/// <param name="serviceID">The service identifier.</param>
		/// <param name="name">Name of the station as shown to the user.</param>
		/// <param name="video">Video PID.</param>
		/// <param name="audio">Audio PID.</param>
		/// <param name="ac3">AC3 PID.</param>
		/// <param name="ttx">TeleText PID.</param>
		/// <param name="pcr">Program Clock Reference PID.</param>
		/// <param name="audioMap">Maps audio names to the related stream identifiers.</param>
		/// <param name="transponderName">Name of the transponder.</param>
		/// <param name="encrypted">Set to create an encrypted station.</param>
		public Station(Transponder transponder, ushort networkID, ushort transportID, ushort serviceID, string name, string transponderName, ushort video, ushort audio, ushort pcr, ushort ac3, ushort ttx, Hashtable audioMap, bool encrypted) : base(networkID, transportID, serviceID)
		{
			// Must hava a transponder and a name
			if ( null == transponder ) throw new ArgumentNullException("transponder");
			if ( (null == name) || (name.Length < 1) ) throw new ArgumentNullException("name");

			// Remember
            TransponderName = transponderName;
            Transponder = transponder;
			Encrypted = encrypted;
			m_AudioMap = audioMap;
			VideoPID = video;
			AudioPID = audio;
            AC3PID = ac3;
			PCRPID = pcr;
			TTXPID = ttx;
			Name = name;

			// Link into transponder
            Transponder.AddStation(this);
		}

		/// <summary>
		/// Clone a station an assign it to a new transponder.
		/// </summary>
		/// <param name="transponder">The target transponder.</param>
		/// <param name="other">The prototype station.</param>
		public Station(Transponder transponder, Station other)
			: base(other.NetworkIdentifier, other.TransportStreamIdentifier, other.ServiceIdentifier)
		{
			// Must hava a transponder and a name
			if (null == transponder) throw new ArgumentNullException("transponder");

			// Remember
			TransponderName = other.TransponderName;
			m_AudioMap = other.m_AudioMap;
			VideoType = other.VideoType;
			Encrypted = other.Encrypted;
			Transponder = transponder;
			VideoPID = other.VideoPID;
			AudioPID = other.AudioPID;
			PCRPID = other.PCRPID;
			TTXPID = other.TTXPID;
			AC3PID = other.AC3PID;
			Name = other.Name;

			// Link into transponder
			Transponder.AddStation(this);
		}

		/// <summary>
		/// 
		/// </summary>
		[XmlElement("AudioInfo")]
		public AudioInfo[] AudioInfos
		{
			get
			{
				// Helper
				List<AudioInfo> infos = new List<AudioInfo>();

				// Fill
				foreach (DictionaryEntry entry in m_AudioMap)
				{
					// Store
					infos.Add(new AudioInfo((ushort)entry.Value, (string)entry.Key));
				}

				// Report
				return infos.ToArray();
			}
			set
			{
				// Reset
				m_AudioMap.Clear();

				// Fill
				if (null != value)
					foreach (AudioInfo info in value)
						m_AudioMap[info.Name] = info.PID;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioPID"></param>
		/// <returns></returns>
		public string FindISOLanguage(ushort audioPID)
		{
			// Loop over
			foreach (DictionaryEntry audio in m_AudioMap)
				if (audioPID == (ushort)audio.Value)
					return FindISOLanguage((string)audio.Key);

			// Not found
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="audioName"></param>
		/// <returns></returns>
		public static string FindISOLanguage(string audioName)
		{
			// Not possible
			if (string.IsNullOrEmpty(audioName)) return null;

			// Split off
			string[] parts = audioName.Split(' ', '[');

			// Find it
			string shortName;
			if (LanguageMap.TryGetValue(parts[0], out shortName)) return shortName;

			// Find it
			if (EnglishLanguageMap.TryGetValue(parts[0], out shortName)) return shortName;

			// Not found
			return null;
		}

		/// <summary>
		/// Report the current mapping of native names to ISO names.
		/// </summary>
		public static Dictionary<string, string> LanguageMap
		{
			get
			{
				// Lock the map
				LoadLanguageMap();

				// Report
				return m_LanguageMap;
			}
		}

		/// <summary>
		/// Report the current mapping of native names to ISO names.
		/// </summary>
		public static Dictionary<string, string> EnglishLanguageMap
		{
			get
			{
				// Lock the map
				LoadLanguageMap();

				// Report
				return m_LanguageMapEx;
			}
		}

		/// <summary>
		/// Fill the language mappings once.
		/// </summary>
		private static void LoadLanguageMap()
		{
			// Lock the map and load
			lock (m_LanguageMap)
				if (m_LanguageMap.Count < 1)
					foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.AllCultures))
					{
						// Primary
						m_LanguageMap[info.NativeName] = info.ThreeLetterISOLanguageName;

						// Extended
						m_LanguageMapEx[info.EnglishName] = info.ThreeLetterISOLanguageName;
					}
		}

		/// <summary>
		/// Report the name for alternate audio streams.
		/// <seealso cref="FindAudio"/>
		/// </summary>
        [XmlIgnore]
        public string[] AudioNames
		{
			get
			{
				// None
				if ( null == m_AudioMap ) return new string[0];

				// Create helper
				string[] names = new string[m_AudioMap.Keys.Count];

				// Fill
				m_AudioMap.Keys.CopyTo(names, 0);

				// Report
				return names;
			}
		}

		/// <summary>
		/// Locate an audio stream identifier by its name.
		/// <seealso cref="AudioNames"/>
		/// </summary>
		/// <param name="audioName">Some valid name of an audio stream.</param>
		/// <returns>The corresponding audio stream identifier.</returns>
		public ushort FindAudio(string audioName)
		{
			// Locate audio by name
			if ( null == m_AudioMap ) throw new ArgumentException("no audio " + audioName, "audioName");

			// Report
			return (ushort)m_AudioMap[audioName];
		}

		/// <summary>
		/// Construct a full (unique) name from the name of the station
		/// and the name of the transponder.
		/// </summary>
        [XmlIgnore]
        public string FullName
		{
			get
			{
				// Construct
                return Name + " [" + TransponderName + "]";
			}
		}
	}
}
