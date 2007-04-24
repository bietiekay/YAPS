using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// RTP packet Handling; at this point there's just one method to delete the header of each RTP packet and
    /// just get the data...
    /// </summary>
    class rtp
    {
        #region KillRTPheader
        /// <summary>
        /// removes a RTP header from an RTP packet
        /// Attention: This does not check if it's actually an RTP packet or not... Hallo mein Schatz!!!
        /// </summary>
        /// <param name="b">incoming buffer with packet</param>
        /// <param name="inlength">length of packet data</param>
        /// <returns>a new buffer with the RTP data and without the header and a new length</returns>
        public static byte[] killRTPheader(byte[] b, ref int inlength)
        {
            // buffer allocation
            byte[] outbytes = new byte[1600];

            // I just copied this the ((b[0]>>0...) stuff from the linux dvbtools rtp.c file...
            // it looks weird but hey, it works...
            int headersize = 12 + 4 * ((b[0] >> 0) & 0x0f);

            // ContrainedCopy is not available in mono; maybe we should do something else in the future here

            // TODO: replace ConstrainedCopy with something else...(MONO)
            Array.ConstrainedCopy(b, headersize, outbytes, 0, inlength - headersize);
            inlength = inlength - headersize;
            return outbytes;
        }
        #endregion
    }
}