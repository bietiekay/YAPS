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
            Bitmap newBitmap = new Bitmap(400, 400);

            Graphics objGraphics;
            Font objFont;

            objGraphics = Graphics.FromImage(newBitmap);

            objGraphics.FillRegion(Brushes.Black, new Region(new Rectangle(0, 0, 400, 400)));

            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            objFont = new System.Drawing.Font("Corbel", 20, FontStyle.Bold);
            objGraphics.DrawString("NEU", objFont, System.Drawing.Brushes.White, 1, 2);

            // Clean up.
            objGraphics.DrawImage(newBitmap, 400, 400);

            newBitmap.Save("test.png", ImageFormat.Png);
        }
    }
}
