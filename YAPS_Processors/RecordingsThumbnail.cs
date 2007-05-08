using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// Creates a thumbnail
    /// </summary>
    public static class RecordingsThumbnail
    {
        public static void CreateRecordingsThumbnail(Recording recording, String Filename)
        {
            Bitmap newBitmap = new Bitmap(182, 256);

            Graphics objGraphics;
            Font objFont;

            objGraphics = Graphics.FromImage(newBitmap);
            // enable text antialiasing
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // fill with white
            objGraphics.FillRegion(Brushes.White, new Region(new Rectangle(0, 0, 182, 256)));

            objFont = new System.Drawing.Font("Constantia", 22, FontStyle.Bold);

            // load graphic
            //Bitmap stationlogo = new Bitmap("station.png");

            objGraphics.DrawString(recording.StartsAt.ToShortDateString(), objFont, System.Drawing.Brushes.Black, 15, 62);
            objGraphics.DrawString(recording.StartsAt.ToShortTimeString(), objFont, System.Drawing.Brushes.Black, 56, 93);
            objGraphics.DrawString("-", objFont, System.Drawing.Brushes.Black, 86, 120);
            objGraphics.DrawString(recording.EndsAt.ToShortTimeString(), objFont, System.Drawing.Brushes.Black, 56, 145);

            objGraphics.DrawRectangle(Pens.Black, new Rectangle(45, 207, 101, 21));

            if ( (recording.LastStoppedPosition != 0) && (recording.FileSize != 0) )
            {

                int percentage = Convert.ToInt32(((float)recording.LastStoppedPosition / (float)recording.FileSize) * 100);

                objGraphics.FillRectangle(Brushes.Red, new Rectangle(46, 208, percentage, 20));
            }

            // Clean up.
            objGraphics.DrawImage(newBitmap, 182, 256);

            newBitmap.Save(Filename, ImageFormat.Png);
        }
    }
}
