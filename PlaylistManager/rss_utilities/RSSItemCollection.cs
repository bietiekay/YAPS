namespace YAPS.RSS
{
	public class RSSItemCollection : System.Collections.CollectionBase
	{
		/// <summary>
		/// Indexer.
		/// </summary>
		public RSSItem this[int index]
		{
			get
			{
				return((RSSItem) List[index]);
			}
		}

		/// <summary>
		/// Default Constructor.
		/// </summary>
		public RSSItemCollection()
		{
		}

		/// <summary>
		/// Add new a RSSItem to collection.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int Add(RSSItem item)
		{
			return(List.Add(item));
		}

		/// <summary>
		/// Add new a RSSItem to collection.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public int Add(string title)
		{
			RSSItem oRSSItem = new RSSItem(title);

			return(Add(oRSSItem));
		}

		/// <summary>
		/// Add new a RSSItem to collection.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="link"></param>
		/// <returns></returns>
		public int Add(string title, string link)
		{
			RSSItem oRSSItem = new RSSItem(title, link);

			return(Add(oRSSItem));
		}

		/// <summary>
		/// Remove a RSSItem from collection.
		/// </summary>
		/// <param name="item"></param>
		public void Remove(RSSItem item)
		{
			List.Remove(item);
		}

		/// <summary>
		/// Find a RSSItem by its Title from collection.
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public RSSItem FindByTitle(string title)
		{
			int intIndex = 0;
			int intFoundIndex = -1;

			while((intIndex <= List.Count - 1) && (intFoundIndex == -1))
			{
				if(((RSSItem) List[intIndex]).Title == title)
					intFoundIndex = intIndex;

				intIndex++;
			}

			if(intFoundIndex == -1)
				return(null);
			else
				return(this[intFoundIndex]);
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
