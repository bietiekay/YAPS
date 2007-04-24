namespace YAPS.RSS
{
	public sealed class RSSUtilities
	{
		private RSSUtilities(){}

		/// <summary>
		/// Create RSSRoot and write it to stream.
		/// </summary>
		/// <param name="rSSRoot"></param>
		/// <returns></returns>
		public static bool PublishRSS(RSSRoot rSSRoot)
		{
			bool blnResult = true;

			if(rSSRoot == null)
				return(false);

			if(rSSRoot.Channel == null)
				return(false);

			System.Xml.XmlTextWriter oXmlTextWriter = new System.Xml.XmlTextWriter(rSSRoot.OutputStream, System.Text.Encoding.UTF8);

			oXmlTextWriter.WriteStartDocument();

			oXmlTextWriter.WriteStartElement("rss");
			oXmlTextWriter.WriteAttributeString("version", "2.0");

			oXmlTextWriter.WriteStartElement("channel");

			oXmlTextWriter.WriteElementString("link", rSSRoot.Channel.Link);
			oXmlTextWriter.WriteElementString("title", rSSRoot.Channel.Title);
			oXmlTextWriter.WriteElementString("description", rSSRoot.Channel.Description);

			if(rSSRoot.Channel.Docs != "")
				oXmlTextWriter.WriteElementString("docs", rSSRoot.Channel.Docs);

			if(rSSRoot.Channel.PubDate != "")
			{
				System.DateTime sDateTime = System.Convert.ToDateTime(rSSRoot.Channel.PubDate);
				oXmlTextWriter.WriteElementString("pubDate", sDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss G\\MT"));
			}

			if(rSSRoot.Channel.Generator != "")
				oXmlTextWriter.WriteElementString("generator", rSSRoot.Channel.Generator);

			if(rSSRoot.Channel.WebMaster != "")
				oXmlTextWriter.WriteElementString("webMaster", rSSRoot.Channel.WebMaster);

			if(rSSRoot.Channel.Copyright != "")
				oXmlTextWriter.WriteElementString("copyright", rSSRoot.Channel.Copyright);

			if(rSSRoot.Channel.LastBuildDate != "")
			{
				System.DateTime sDateTime = System.Convert.ToDateTime(rSSRoot.Channel.LastBuildDate);
				oXmlTextWriter.WriteElementString("lastBuildDate", sDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss G\\MT"));
			}

			if(rSSRoot.Channel.ManagingEditor != "")
				oXmlTextWriter.WriteElementString("managingEditor", rSSRoot.Channel.ManagingEditor);

			oXmlTextWriter.WriteElementString("language", rSSRoot.Channel.Language.ToString().Replace("_", "-"));

			if(rSSRoot.Image != null)
			{
				oXmlTextWriter.WriteStartElement("image");

				oXmlTextWriter.WriteElementString("url", rSSRoot.Image.URL);
				oXmlTextWriter.WriteElementString("link", rSSRoot.Image.Link);
				oXmlTextWriter.WriteElementString("title", rSSRoot.Image.Title);

				if(rSSRoot.Image.Description != "")
					oXmlTextWriter.WriteElementString("description", rSSRoot.Image.Description);

				if(rSSRoot.Image.Width != 0)
					oXmlTextWriter.WriteElementString("width", rSSRoot.Image.Width.ToString());

				if(rSSRoot.Image.Height != 0)
					oXmlTextWriter.WriteElementString("height", rSSRoot.Image.Height.ToString());

				oXmlTextWriter.WriteEndElement();
			}

			foreach(RSSItem itmCurrent in rSSRoot.Items)
			{
				oXmlTextWriter.WriteStartElement("item");

				if(itmCurrent.Link != "")
					oXmlTextWriter.WriteElementString("link", itmCurrent.Link);

				if(itmCurrent.Title != "")
					oXmlTextWriter.WriteElementString("title", itmCurrent.Title);

				if(itmCurrent.Author != "")
					oXmlTextWriter.WriteElementString("author", itmCurrent.Author);

				if(itmCurrent.PubDate != "")
				{
					System.DateTime sDateTime = System.Convert.ToDateTime(itmCurrent.PubDate);
					oXmlTextWriter.WriteElementString("pubDate", sDateTime.ToString("ddd, dd MMM yyyy HH:mm:ss G\\MT"));
				}

				if(itmCurrent.Comment != "")
					oXmlTextWriter.WriteElementString("comment", itmCurrent.Comment);

				if(itmCurrent.Description != "")
					oXmlTextWriter.WriteElementString("description", itmCurrent.Description);

                if(itmCurrent.Categories != "")
					oXmlTextWriter.WriteElementString("category", itmCurrent.Categories);

				oXmlTextWriter.WriteEndElement();
			}
			oXmlTextWriter.WriteEndElement();
			oXmlTextWriter.WriteEndElement();

			oXmlTextWriter.WriteEndDocument();

            // TODO: rewrite the XML Textwriter stuff
			oXmlTextWriter.Flush();
//			oXmlTextWriter.Close();

			return(blnResult);
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
