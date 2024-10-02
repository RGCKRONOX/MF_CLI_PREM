using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class ErrorDTO
    {
        
        public int code { get; set; }
        public string message { get; set; }
    }
}
