using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorComercialCLI.DTOs
{
    public class ComercialSdkConsumerConfigDTO
    {
        public EComercialSdkConsumerMode mode = EComercialSdkConsumerMode.DIRECTLY_PARAMS;
        public string userSDK;
        public string passSDK;
    }
}
