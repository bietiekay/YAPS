using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace YAPS.WCF
{
    [ServiceContract]
    public interface IYAPSService
    {
        [OperationContract]
        String YAPSVersion();
    }
}
