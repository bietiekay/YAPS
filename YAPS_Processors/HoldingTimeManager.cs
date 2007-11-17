using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public static class HoldingTimeManager
    {
        public static Int32 HowOldIsThisRecordingInDays(DateTime ToBeCompared)
        {
            Int64 ComparedTicks = DateTime.Now.Ticks - ToBeCompared.Ticks;

            Int64 OneDay = 864000000000;

            return (Int32)(ComparedTicks / OneDay);
        }
    }
}
