using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    // Credit: Dumitru Bogdan, 2004
    public class TSProcessor_PacketData
    {
        public byte[] data;	// Pack data
        public int size;	// Pack length
        public TSProcessor_PacketData(int len)
        {
            size = len;
            data = new byte[size];
        }
    }
}
