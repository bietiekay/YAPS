using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Text;

namespace CreateThumb
{
    class Program
    {
        static void Main(string[] args)
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

            objGraphics.DrawString("16.05.2007", objFont, System.Drawing.Brushes.Black, 15, 62);
            objGraphics.DrawString("16:30", objFont, System.Drawing.Brushes.Black, 56, 93);
            objGraphics.DrawString("-", objFont, System.Drawing.Brushes.Black, 86, 120);
            objGraphics.DrawString("17:00", objFont, System.Drawing.Brushes.Black, 56, 145);

            objGraphics.DrawRectangle(Pens.Black, new Rectangle(45, 207, 101, 21));

            objGraphics.FillRectangle(Brushes.Red, new Rectangle(46, 208, 100, 20));

            // Clean up.
            objGraphics.DrawImage(newBitmap, 182, 256);

            newBitmap.Save("test.png", ImageFormat.Png);
        }
    }
}
