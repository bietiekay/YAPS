using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TSSplitter
{
	public class EPGEntry: ListViewItem
	{
		public class Comparer : System.Collections.IComparer
		{
			public Comparer()
			{
			}

			#region IComparer Members

			public int Compare(object x, object y)
			{
				// Change type
				EPGEntry leftObject = x as EPGEntry;
				EPGEntry rightObject = y as EPGEntry;

				// Not sortable
				if (null == rightObject) return (null == leftObject) ? 0 : -1;
				if (null == leftObject) return +1;

				// Attach to parent
				ReaderMain main = (ReaderMain)leftObject.ListView.Parent;

				// Get the sort index
				int sortIndex = main.SortIndex;
				bool ascending = (sortIndex > 0);

				// Correct
				if (!ascending) sortIndex = -sortIndex;

				// Load
				IComparable left = (IComparable)leftObject.CompareData[--sortIndex];
				object right = rightObject.CompareData[sortIndex];

				// Process
				int result = left.CompareTo(right);

				// Correct
				return ascending ? result : -result;
			}

			#endregion
		}

		public readonly object[] CompareData;

		public EPGEntry(ushort service, string name, string description, DateTime start, TimeSpan duration)
		{
			// Main
			Text = string.Format("{0} (0x{0:x})", service);

			// Load all
			SubItems.Add(start.ToString());
			SubItems.Add(start.Add(duration).TimeOfDay.ToString());
			SubItems.Add(name);
			SubItems.Add(description);

			// Create for compare
			CompareData = new object[] { service, start, start.Add(duration), name };
		}
	}
}