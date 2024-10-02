using ConectorComercialCLI.DTOs;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ConectorComercialCLI.DTOs.Existencias
{
    public class ConsultaExistenciasProductoRespuestaDto
    {
        public string codProducto {  get; set; }
        public List<ExistenciasAlmacenDto> existencias { get; set; }
    }
}
