using System;
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
            ConsoleOutputLogger.WriteLine("SAP / SDP Processor up and running...");
            // wait for the settings to be loaded...
            while (!done)
            {
                Thread.Sleep(10);
            }
        }
    }
}
