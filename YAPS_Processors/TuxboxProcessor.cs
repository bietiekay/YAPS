using System;
using System.Collections.Generic;
using System.Text;

namespace YAPS
{
    public static class TuxboxProcessor
    {
        /// <summary>
        /// This method adds a "Currently Running" bouquet to the bouquet datastructure given as a parameter
        /// </summary>
        /// <param name="currentbouquets"></param>
        /// <param name="EPG_Processor"></param>
        public static tuxbox.bouquet addCurrentlyRunningBouquet(multicastedEPGProcessor EPG_Processor)
        {
            try
            {
                lock (EPG_Processor.CurrentlyRunningEvents)
                {
                    if (EPG_Processor.CurrentlyRunningEvents.Count > 0)
                    {
                        tuxbox.bouquet newbouquet = new tuxbox.bouquet();

                        newbouquet.name = "Currently Running";
                        newbouquet.reference = "4097:7:0:33fc5:0:0:0:0:0:0:/etc/enigma/currently_running.33fc5.tv";

                        newbouquet.service = new YAPS.tuxbox.service[EPG_Processor.CurrentlyRunningEvents.Count];

                        int i = 0;

                        foreach (EPG_Event_Entry entry in EPG_Processor.CurrentlyRunningEvents)
                        {
                            newbouquet.service[i] = new YAPS.tuxbox.service();
                            newbouquet.service[i].name = entry.ShortDescription.Name;
                            newbouquet.service[i].reference = "1:0:1:6d66:437:1:c00000:0:0:0:";
                            newbouquet.service[i].provider = ChannelAndStationMapper.ServiceID2Name(entry.Service);
                            newbouquet.service[i].orbital_position = entry.Service.ToString();
                            i++;
                        }
                        return newbouquet;
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleOutputLogger.WriteLine("TuxboxProcessor.addCurrentlyRunningBouquet: " + e.Message);
            }
            return null;
        }
    }
}
