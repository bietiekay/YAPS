using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// This class is a quick and dirty hack to sort the Recording/DoneRecording Tables by StartAt time...
    /// </summary>
    public static class Sorter
    {
        public static List<Recording> SortRecordingTable(Hashtable RecordingTable, bool ascending)
        {

            List<Recording> SortedList = new List<Recording>();

            // now do some InsertionSort style sorting
            foreach (Recording element in RecordingTable.Values)
            {
                if (SortedList.Count == 0)
                    SortedList.Insert(0, element);
                else
                {
                    bool inserted = false;
                    // find a place to insert it..
                    for (int i = 0; i < SortedList.Count; i++)
                    {
                        if (ascending)
                        {
                            if (SortedList[i].StartsAt.Ticks > element.StartsAt.Ticks)
                            {
                                inserted = true;
                                SortedList.Insert(i, element);
                                break;
                            }
                        }
                        else
                        {
                            if (SortedList[i].StartsAt.Ticks < element.StartsAt.Ticks)
                            {
                                inserted = true;
                                SortedList.Insert(i, element);
                                break;
                            }
                        }
                    }
                    if (!inserted)
                        SortedList.Add(element);
                }
            }

            return SortedList;
        }
    }
}
