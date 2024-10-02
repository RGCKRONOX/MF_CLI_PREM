using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ConectorComercialCLI.DTOs.Almacenes
{
    public class ConsultaAlmacenesRespuestaDto
    {
        public List<AlmacenDto> almacenes {  get; set; }
        public ErrorDTO error {  get; set; }
    }
}
