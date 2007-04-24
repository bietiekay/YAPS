namespace YAPS.RSS
{
	public class RSSRoot
	{
		private System.IO.Stream _outputStream;

		private RSSImage _image;
		private RSSChannel _channel;
		private RSSItemCollection _items = new RSSItemCollection();

		/// <summary>
		/// The XML context will be written in this stream.
		/// </summary>
		public System.IO.Stream OutputStream
		{
			get
			{
				return(_outputStream);
			}
			set
			{
				_outputStream = value;
			}
		}

		/// <summary>
		/// Image Tag.
		/// </summary>
		public RSSImage Image
		{
			get
			{
				return(_image);
			}
		}

		/// <summary>
		/// Channel Tag.
		/// </summary>
		public RSSChannel Channel
		{
			get
			{
				return(_channel);
			}
		}

		/// <summary>
		/// Items Tag.
		/// </summary>
		public RSSItemCollection Items
		{
			get
			{
				return(_items);
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="outputStream"></param>
		public RSSRoot(RSSChannel channel, System.IO.Stream outputStream)
		{
			_channel = channel;
			_outputStream = outputStream;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="image"></param>
		/// <param name="outputStream"></param>
		public RSSRoot(RSSChannel channel, RSSImage image, System.IO.Stream outputStream)
		{
			_image = image;
			_channel = channel;
			_outputStream = outputStream;
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
