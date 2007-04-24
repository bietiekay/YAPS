namespace YAPS.RSS
{
	public class RSSImage
	{
		private string _uRL = "";
		private string _link = "";
		private string _title = "";
		private string _description = "";

		private int _width = 0;
		private int _height = 0;

		/// <summary>
		/// [Required] - The URL of the image file.
		/// </summary>
		public string URL
		{
			get
			{
				return(_uRL);
			}
			set
			{
				_uRL = value;
			}
		}

		/// <summary>
		/// [Required] - The URL of the site, when the channel is rendered, the image is a link to the site. (Note, in practice the image <title> and <link> should have the same value as the channel's <title> and <link> 
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
		/// [Required] - Describes the image, it's used in the ALT attribute of the HTML <img> tag when the channel is rendered in HTML.
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
		/// [Optional] - Text that is included in the TITLE attribute of the link formed around the image in the HTML rendering.
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
		/// [Optional] - The width of the image in pixels. Default value is 0. Maximum value must be 400.
		/// </summary>
		public int Width
		{
			get
			{
				return(_width);
			}
			set
			{
				if(_width < 400)
					_width = value;
			}
		}

		/// <summary>
		/// [Optional] - The height of the image in pixels. Default value is 0. Maximum value must be 144.
		/// </summary>
		public int Height
		{
			get
			{
				return(_height);
			}
			set
			{
				if(_width < 144)
					_height = value;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="url"></param>
		/// <param name="link"></param>
		/// <param name="title"></param>
		public RSSImage(string url, string link, string title)
		{
			_uRL = url;
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
