using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using YAPS;

namespace YAPS.WCF
{
    public class YAPSService : IYAPSService
    {
        public string YAPSVersion()
        {
            return "0.1";
        }
    }
}
