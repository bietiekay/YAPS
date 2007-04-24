namespace YAPS.RSS
{
	public class RSSItem
	{
		private string _link = "";
		private string _title = "";
		private string _author = "";
		private string _pubDate = "";
		private string _comment = "";
		private string _description = "";

		/// <summary>
		/// [Optional] - The URL of the item.
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
		/// [Optional] - The title of the item.
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
		/// [Optional] - Email address of the author of the item.
		/// </summary>
		public string Author
		{
			get
			{
				return(_author);
			}
			set
			{
				_author = value;
			}
		}

		/// <summary>
		/// [Optional] - Indicates when the item was published. RFC 822 Formated date.
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
		/// [Optional] - URL of a page for comments relating to the item.
		/// </summary>
		public string Comment
		{
			get
			{
				return(_comment);
			}
			set
			{
				_comment = value;
			}
		}

		/// <summary>
		/// [Optional] - The item synopsis.
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
		/// Default Constructor
		/// </summary>
		public RSSItem()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public RSSItem(string title)
		{
			_title = title;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		public RSSItem(string title, string link)
		{
			_link = link;
			_title = title;
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
