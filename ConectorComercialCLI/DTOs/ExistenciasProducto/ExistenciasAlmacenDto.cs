using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ConectorComercialCLI.DTOs.Existencias
{
    public class ExistenciasAlmacenDto
    {
        public double existencias { get; set; }
        public string codAlmacen {  get; set; }
        public ErrorDTO error { get; set; }
    }
}
