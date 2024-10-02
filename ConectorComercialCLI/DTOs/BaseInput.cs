using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class BaseInput  {
        public string userSDK { get; set; }
        public string passSDK { get; set; }
        public string instancia { get; set; }
        public string userSQL { get; set; }
        public string passSQL { get; set; }
        public string empresaDB { get; set; }
        public string rutaEmpresa { get; set; }
        public string PassCSD { get; set; }
    }
}
