using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class MovimientoDocumentoDTO
    {
        //[JsonProperty(Required = Required.AllowNull)]
        public string id { get; set; }
        public string consecutivo { get; set; }
        public string unidades { get; set; }
        public string precio { get; set; }

        //[JsonProperty(Required = Required.AllowNull)]
        public string costo { get; set; }
        public string codProdSer { get; set; }
        public string codAlmacen { get; set; }
        public string referencia { get; set; }
        public string ctextoExtra1 { get; set;}
    }
}
