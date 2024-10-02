using ConectorComercialCLI.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs.Existencias
{
    public class InputConsultaExistenciaProductoDTO : BaseInput
    {
        public ConsultaExistenciasProductoDto[] productos {  get; set; }
    }
}
