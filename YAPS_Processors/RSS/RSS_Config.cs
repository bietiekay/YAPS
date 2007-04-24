using System;
using System.Collections.Generic;
using System.Text;

/*
 * YAPS.RSS is based upon the article: http://www.codeproject.com/aspnet/Working_on_RSS_20.asp
 * Thanks to Dariush Tasdighi
 * */
namespace YAPS.RSS
{
    /// <summary>
    /// This class contains the common information about the rss feed...
    /// </summary>
    class RSS_Config
    {
		public static string Owner
		{
			get
			{
				return("YAPS VCR Server");
			}
		}

		public static string Version
		{
			get
			{
                return (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
			}
		}

		public static string Support
		{
			get
			{
				return("btk@technology-ninja.com");
			}
		}

		public static string Homepage
		{
			get
			{
                return ("");
			}
		}

		public static string UpdatedDate
		{
			get
			{
				return(DateTime.Now.ToShortDateString());
			}
		}
    }
}
