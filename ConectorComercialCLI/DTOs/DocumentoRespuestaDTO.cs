using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class DocumentoRespuestaDTO : DocumentoDTO
    {
        //error
        public ErrorDTO error { get; set; }
    }
}

