using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAPS
{
    public class SAPProcessor
    {
        private Int32 SAP_Port;
        private String SAP_IPAdress;
        private bool done;

        public SAPProcessor(String IPAdress, Int32 Port)
        {
            SAP_Port = Port;
            SAP_IPAdress = IPAdress;
            done = false;
        }

        #region Flags and stuff
        /// <summary>
        /// when this is called the Scheduler Thread will shut down the next turnaround
        /// </summary>
        public void youreDone()
        {
            done = true;
        }
        #endregion

        public void SAPProcessorThread()
        {
            // wait for the settings to be loaded...
            Socket MulticastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, SAP_Port);
            IPAddress ip = IPAddress.Parse(SAP_IPAdress);

            MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
            MulticastSocket.Bind(ipep);
            MulticastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ip, IPAddress.Any));

            #region we need some memory to store data withing
            byte[] receiveBuffer = new byte[2048];
            int receivedBufferLength = 0;
            #endregion

            // start the multicast client
            ConsoleOutputLogger.WriteLine("SAP / SDP Processor up and running...");

            bool done = false;
            Int32 ErrorCounter = 0;
            SDP_Processor sdp_processor = new SDP_Processor();

            while (!done)
            {
                // receive data
                receivedBufferLength = MulticastSocket.Receive(receiveBuffer);

                try
                {
                    //binary_recorder_writer.Write(receiveBuffer, 0, receivedBufferLength);
                    SDPPacket data = sdp_processor.ProcessSDP_Packet(receiveBuffer, receivedBufferLength);
                    ErrorCounter = 0;
                    //Console.WriteLine("Sender: " + data.Name);
                    //Console.WriteLine("IP: " + data.IP_Adress);
                    //Console.WriteLine("Port: " + data.Port);
                    //for (int i = 0; (i + epgPacketSize) <= receivedBufferLength; i += epgPacketSize) ProcessEPGPacket(receiveBuffer, i);                
                }
                catch (Exception e)
                {
                    ConsoleOutputLogger.WriteLine("SAP / SDP Processor threw an error: " + e.Message);
                    ErrorCounter++;
                }
                // waiting FTW!!
                Thread.Sleep(10);

                // end this thread when 25 errors in a line occured
                if (ErrorCounter == 25)
                {
                    ConsoleOutputLogger.WriteLine("Too many errors in SAP / SDP Processor - shutting it down.");
                    youreDone();
                }
            }
            MulticastSocket.Close();
        }
    }
}
