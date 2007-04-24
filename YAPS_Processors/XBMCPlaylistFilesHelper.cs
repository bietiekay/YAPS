using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public static class XBMCPlaylistFilesHelper
    {
        public static bool IfPathExists(string Path)
        {
            try
            {
                if (!Directory.Exists(Path))
                {
                    Directory.CreateDirectory(Path);
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("RecordingsManager Exception: " + e.Message);
                return false;
            }
            return true;
        }

        #region PlayListFilenames
        public static String generateCurrentlyRecordingPlaylistFilename(Recording _recording)
        {
            String Output = "";

            if (_recording != null)
            {
                if (IfPathExists(".\\Playlists\\currentlyRecording\\"))
                {
                    //Output = ".\\Playlists\\currentlyRecording\\" + _recording.Recording_Name.Replace(":", " -").Replace('?', '_').Replace('/', '_').Replace('\\', '_').Replace('\'', '_').Replace('\"', '_').Replace('&', '_').Replace('>', '_').Replace('<', '_').Replace("?", "") + "              " + _recording.Recording_ID + ".strm";
                    Output = ".\\Playlists\\currentlyRecording\\" + _recording.Recording_Name.Replace(":", " -").Replace('?', '_').Replace('/', '_').Replace('\\', '_').Replace('\'', '_').Replace('\"', '_').Replace('&', '_').Replace('>', '_').Replace('<', '_').Replace("?", "") + "                                          " + _recording.Recording_ID + ".strm";
                }
            }
            return Output;
        }

        public static bool removeCurrentlyRecordingPlaylistFilename(Recording _recording)
        {
            try
            {
                if (File.Exists(generateCurrentlyRecordingPlaylistFilename(_recording)))
                {
                    File.Delete(generateCurrentlyRecordingPlaylistFilename(_recording));
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("XBMCPlaylistFilesHelper Exception: "+e.Message);
                return false;
            }
            return true;
        }

        public static String generatePlaylistFilename(Recording _recording)
        {
            String Output = "";

            if (_recording != null)
            {
                if (IfPathExists(".\\Playlists\\"))
                {
                    Output = ".\\Playlists\\" + _recording.Recording_Name.Replace(":", " -").Replace('?', '_').Replace('/', '_').Replace('\\', '_').Replace('\'', '_').Replace('\"', '_').Replace('&', '_').Replace('>', '_').Replace('<', '_').Replace("?", "") + "                                          " + _recording.Recording_ID + ".strm";
                }
            }
            return Output;
        }
        #endregion
    }
}
