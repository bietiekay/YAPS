using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAPS
{
    /// <summary>
    /// This is the Session Description Protocol Processor which parses Inputdata and extractes
    /// the information.
    /// </summary>
    public class SDP_Processor
    {
        #region ProcessSDP Packet
        /// <summary>
        /// Parses the InputData and returns a SDPPacket data structure or null if any error occured
        /// </summary>
        /// <param name="InputData">the byte array that holds the data</param>
        /// <param name="Datalength">the length of the data</param>
        /// <returns>an SDPPacket data structure or null of any error occured</returns>
        public SDPPacket ProcessSDP_Packet(byte[] InputData, Int32 Datalength)
        {
            MemoryStream ms = new MemoryStream(InputData, 0, Datalength);
            StreamReader sr = new StreamReader(ms);

            String line;
            SDPPacket OutputData = new SDPPacket();

            String[] Splitted;

            while ((line = sr.ReadLine()) != null)
            {
                Splitted = line.Split('=');

                try
                {
                    if (Splitted[0] == "s")
                        OutputData.Name = Splitted[1];

                    if (Splitted[0] == "m")
                    {
                        Splitted = Splitted[1].Split(' ');
                        OutputData.Port = Splitted[1];
                    }

                    if (Splitted[0] == "c")
                    {
                        Splitted = Splitted[1].Split(' ');
                        OutputData.IP_Adress = Splitted[2].Split('/')[0];
                    }
                }
                catch (Exception e)
                {
                    ConsoleOutputLogger.WriteLine("SDP Parser Error: " + e.Message);
                    return null;
                }
            }
            OutputData.TimeStamp = DateTime.Now;

            return OutputData;
        }
        #endregion
    }
}
