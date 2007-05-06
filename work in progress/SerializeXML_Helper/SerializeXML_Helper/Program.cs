using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text;
using YAPS.tuxbox;

namespace SerializeXML_Helper
{
    class Program
    {
        static void Main(string[] args)
        {
            currentservicedata CurrentServiceData_ = new currentservicedata();

            FileStream fs = new FileStream("output.xml", FileMode.Create, FileAccess.Write);

            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "currentservicedata";
            xRoot.IsNullable = true;

            channel channel = new channel();

            channel.Name = "Stereo";
            channel.pid = "0x01";
            channel.selected = 1;

            CurrentServiceData_.audio_channels.Add(channel);


            CurrentServiceData_.current_event.date = DateTime.Now.ToShortDateString();
            CurrentServiceData_.current_event.description = "Sendungsname";
            CurrentServiceData_.current_event.details = "Beschreibungstext blah blah";
            CurrentServiceData_.current_event.duration = "90";
            CurrentServiceData_.current_event.start = DateTime.Now.ToShortDateString();
            CurrentServiceData_.current_event.time = DateTime.Now.ToShortTimeString();

            CurrentServiceData_.next_event = CurrentServiceData_.current_event;
            CurrentServiceData_.service.name = "Sendername";
            CurrentServiceData_.service.reference = "reference";

            System.Xml.Serialization.XmlSerializer xmls = new XmlSerializer(CurrentServiceData_.GetType(),xRoot);
            xmls.Serialize(fs, CurrentServiceData_);

            fs.Close();
        }
    }
}
