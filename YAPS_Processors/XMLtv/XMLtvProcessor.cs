using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace YAPS.XMLtv
{
    class XMLtvProcessor
    {
        public XMLtv.tv xmltv_data;

        public XMLtvProcessor(String XMLtvFilename)
        {
            if (File.Exists(XMLtvFilename))
            {
                FileStream fs;            
                fs = new FileStream(XMLtvFilename, FileMode.Open, FileAccess.Read);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(XMLtv.tv));
                    xmltv_data = (XMLtv.tv)serializer.Deserialize(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        public DateTime ParseProgrammeDatetime(string ProgrammeDateTime)
        {
            // 20070323042000 +0100
            DateTime output_datetime;

            try
            {
                output_datetime = DateTime.ParseExact(ProgrammeDateTime, "yyyyMMddHHmmss zzzzz", null);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }

            return output_datetime;
        }

        public List<XMLtv.tvProgramme> get_TVProgramme(String Channel)
        {
            List<XMLtv.tvProgramme> allPrograms = new List<YAPS.XMLtv.tvProgramme>();
            foreach (XMLtv.tvProgramme programme in xmltv_data.programme)
            {
                if (programme.channel == Channel)
                    allPrograms.Add(programme);
            }

            return allPrograms;
        }
    }
}