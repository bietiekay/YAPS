using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// holds the data of one particular category
    /// </summary>
    [Serializable]
    public class Category
    {
        public String Name;
        public String Comment;
        public DateTime LastPlayed;
        public DateTime LastChanged;

        public bool isAutomatic;

        public List<String> SearchTerms;

        public int MaximumNumberOfElements;
        public TimeSpan MaximumAge;
        public bool checkMaximumAge;
        public bool checkMaximumNumberOfElements;

        public Category()
        {
            LastPlayed = DateTime.MinValue;
            LastChanged = DateTime.Now;
            SearchTerms = new List<string>();
            isAutomatic = true;
        }
    }

}