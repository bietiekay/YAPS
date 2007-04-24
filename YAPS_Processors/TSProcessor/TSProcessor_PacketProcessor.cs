using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public class TSProcessor_PacketProcessor
    {
        public TSProcessor_PacketData pack;		// Associated packet
        public int len;		// Packet length
        public bool error;		// Transport Error indicator
        public bool pes_st;		// PES Packet start
        public int pid;		// Packet's PID
        public bool AF;			// Adaptation Field existence flag
        public bool Pay;		// Payload existence flag
        public int count;		// Continuity counter
        public int Ad_len;		// Adaptation Field Length
        public bool has_PCR;	// PCR flag
        public double PCR;		// PCR value
        public int Pay_len;	// Payload length
        public bool nul;		// Null Packet indicator
        public int type;		// Packet Type

        // Copy read data as TS Pack
        public void copy_ts(byte[] data, int size)
        {
            len = size;
            pack = new TSProcessor_PacketData(size);
            for (int i = 0; i < len; i++) pack.data[i] = data[i];
        }

        // Extract PCR value
        public double go_to_v()
        {
            ulong v_ref, all;
            int v_ext, i;
            double ret;

            v_ref = 0;
            v_ref = pack.data[6];	// Overflow case -> first byte has max. value
            v_ref = v_ref << 25;
            for (i = 1; i < 4; i++)
                v_ref += Convert.ToUInt64(pack.data[6 + i] << (3 - i) * 8 + 1);
            v_ref += Convert.ToUInt64(pack.data[10] / 128);
            v_ext = 0;
            v_ext = (pack.data[10] % 2) * 256 + pack.data[11];
            all = Convert.ToUInt64(v_ref * 300 + Convert.ToUInt64(v_ext));
            ret = Convert.ToDouble(all) / 27000;
            return ret;
        }

        public void analyze_pack()
        {
            TSProcessor_BitManipulation op = new TSProcessor_BitManipulation();

            error = op.ret_bit(pack.data[1], 0);
            pes_st = op.ret_bit(pack.data[1], 1);
            pid = op.ret_bit_value(pack.data[1], 3, 7) * 256 + op.ret_bit_value(pack.data[2], 0, 7);
            if (pid == 8191) { nul = true; type = -1; return; }	// Null packet
            if (pid == 0) { type = 0; return; }		// PAT
            if (pid == 1) { type = 1; return; }		// CAT
            if ((pid > 1) && (pid < 16)) { type = 2; return; }		// Reserved
            type = 3;					// Normal TS packet
            AF = op.ret_bit(pack.data[3], 2);
            Pay = op.ret_bit(pack.data[3], 3);
            count = op.ret_bit_value(pack.data[3], 4, 7);
            Pay_len = len - 4;
            if (AF)		// Has Adaptation Field
            {
                Ad_len = op.ret_bit_value(pack.data[4], 0, 7);
                has_PCR = op.ret_bit(pack.data[5], 3);
                if (has_PCR)	// Has PCR Time Stamp
                    PCR = go_to_v();
                Pay_len -= Ad_len + 1;
            }

        }

    }
}
