using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public static class RecordingsManager
    {       
        public static bool deleteRecording(Recording _recording, VCRScheduler vcrscheduler)
        {
            try
            {
                lock (vcrscheduler.doneRecordings.SyncRoot)
                {
                    // remove file
                    File.Delete(_recording.Recording_Filename);

                    // remove entry in Hashtable
                    vcrscheduler.doneRecordings.Remove(_recording.Recording_ID);
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("RecordingsManager Exception: "+e.Message);
                return false;
            }
            ConsoleOutputLogger.WriteLine("RecordingsManager: Deleted recording "+_recording.Recording_Name);
            return true;
        }
    }
}
