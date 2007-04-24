using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream eingabedatei = new FileStream(args[0], FileMode.Open);
            FileStream ausgabedatei = new FileStream(args[1], FileMode.CreateNew);

            byte[] Buffer = new byte[188];

            int blength;

            while (eingabedatei.Read(Buffer, 0, 188) != 0)
            {
                blength = 188;
               
                byte[] ob = rtp.killRTPheader(Buffer, ref blength);

                ausgabedatei.Write(ob, 0, blength);
            }
            
        }
    }
}
