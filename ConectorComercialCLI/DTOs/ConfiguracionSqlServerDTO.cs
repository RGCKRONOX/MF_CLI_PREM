using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class ConfiguracionSqlServerDTO
    {
        public string instancia { get; set; }
        public string db { get; set; }
        public string usuario { get; set; }
        public string contrasena { get; set; }
    }
}
