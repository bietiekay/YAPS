using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace YAPS
{
    class Program
    {
        static void Main(string[] args)
        {
            XMLtv.XMLtvProcessor xmltvprocessor = new XMLtv.XMLtvProcessor("a.xml");

            foreach (XMLtv.tvChannel channel in xmltvprocessor.xmltv_data.channel)
            {
                Console.WriteLine("  ");
                Console.WriteLine(" +-- "+channel.displayname[0].Value);
                Console.WriteLine(" |");
                List<XMLtv.tvProgramme> allprogramms = xmltvprocessor.get_TVProgramme(channel.id);

                foreach (XMLtv.tvProgramme program in allprogramms)
                {
                    Console.WriteLine(" |  "+program.title[0].Value);

                    Console.WriteLine(" |  " + xmltvprocessor.ParseProgrammeDatetime(program.start).ToString());
                }
            }
        }
    }
}
