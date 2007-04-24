namespace YAPS.RSS
{
	public class RSSChannel
	{
		private string _docs = "";
		private string _link = "";
		private string _title = "";
		private string _pubDate = "";
		private string _generator = "";
		private string _webMaster = "";
		private string _copyright = "";
		private string _description = "";
		private string _lastBuildDate = "";
		private string _managingEditor = "";

		private RSSLanguage _language = RSSLanguage.en_US;

		/// <summary>
		/// [Optional] - A URL that points to the documentation for the format used in the RSS file. It's probably a pointer to this page. It's for people who might stumble across an RSS file on a Web server 25 years from now and wonder what it is.
		/// </summary>
		public string Docs
		{
			get
			{
				return(_docs);
			}
			set
			{
				_docs = value;
			}
		}

		/// <summary>
		/// [Required] - The URL to the HTML website corresponding to the channel.
		/// </summary>
		public string Link
		{
			get
			{
				return(_link);
			}
			set
			{
				_link = value;
			}
		}

		/// <summary>
		/// [Required] - The name of the channel. It's how people refer to your service. If you have an HTML website that contains the same information as your RSS file, the title of your channel should be the same as the title of your website.
		/// </summary>
		public string Title
		{
			get
			{
				return(_title);
			}
			set
			{
				_title = value;
			}
		}

		/// <summary>
		/// [Optional] - The publication date for the content in the channel. All date-times in RSS conform to the Date and Time Specification of RFC 822, with the exception that the year may be expressed with two characters or four characters (four preferred). RFC 822 Formated date.
		/// </summary>
		public string PubDate
		{
			get
			{
				return(_pubDate);
			}
			set
			{
				_pubDate = value;
			}
		}
		/// <summary>
		/// [Optional] - A string indicating the program used to generate the channel.
		/// </summary>
		public string Generator
		{
			get
			{
				return(_generator);
			}
			set
			{
				_generator = value;
			}
		}

		/// <summary>
		/// [Optional] - Email address for person responsible for technical issues relating to channel.
		/// </summary>
		public string WebMaster
		{
			get
			{
				return(_webMaster);
			}
			set
			{
				_webMaster = value;
			}
		}

		/// <summary>
		/// [Optional] - Copyright notice for content in the channel.
		/// </summary>
		public string Copyright
		{
			get
			{
				return(_copyright);
			}
			set
			{
				_copyright = value;
			}
		}

		/// <summary>
		/// [Required] - Phrase or sentence describing the channel.
		/// </summary>
		public string Description
		{
			get
			{
				return(_description);
			}
			set
			{
				_description = value;
			}
		}

		/// <summary>
		/// [Optional] - The last time the content of the channel changed. RFC 822 Formated date.
		/// </summary>
		public string LastBuildDate
		{
			get
			{
				return(_lastBuildDate);
			}
			set
			{
				_lastBuildDate = value;
			}
		}

		/// <summary>
		/// [Optional] - Email address for person responsible for editorial content.
		/// </summary>
		public string ManagingEditor
		{
			get
			{
				return(_managingEditor);
			}
			set
			{
				_managingEditor = value;
			}
		}

		/// <summary>
		/// [Optional] - The language the channel is written in. This allows aggregators to group all Italian language sites, for example, on a single page. A list of allowable values for this element, as provided by Netscape, is here. You may also use values defined by the W3C.
		/// </summary>
		public RSSLanguage Language
		{
			get
			{
				return(_language);
			}
			set
			{
				_language = value;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="title"></param>
		/// <param name="link"></param>
		/// <param name="description"></param>
		public RSSChannel(string title, string link, string description)
		{
			_link = link;
			_title = title;
			_description = description;
		}

        #region Support
        public static string Owner
        {
            get
            {
                return (RSS_Config.Owner);
            }
        }

        public static string Version
        {
            get
            {
                return (RSS_Config.Version);
            }
        }

        public static string Support
        {
            get
            {
                return (RSS_Config.Support);
            }
        }

        public static string Homepage
        {
            get
            {
                return (RSS_Config.Homepage);
            }
        }

        public static string UpdatedDate
        {
            get
            {
                return (RSS_Config.UpdatedDate);
            }
        }
        #endregion
    }
}
