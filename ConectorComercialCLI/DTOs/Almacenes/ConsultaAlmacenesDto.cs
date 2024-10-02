using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ConectorComercialCLI.DTOs.Almacenes
{
    public class ConsultaAlmacenesDto
    {
        // si no se especifica el almacen se puede especificar un producto para saber cuales son los almacenes donde se encuentra
        public string codProducto { get; set; }
        public string codAlmacen {  get; set; }
    }
}
