using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConectorComercialCLI.DTOs
{
    public class SdkDocTimbrar
    {
        public class Comprobante
        {
            public string aCodConcepto { get; set; }
            public string aSerie { get; set; }
            public string aFolio { get; set; }
            public string aPassword { get; set; }
            public string aArchivoAdicional { get; set; }
        }
    }
}
