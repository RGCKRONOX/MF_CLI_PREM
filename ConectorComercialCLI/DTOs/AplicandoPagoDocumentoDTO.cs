using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class AplicandoPagoDocumentoDTO
    {
       public ErrorDTO error { get; set; }  
       public DocumentoDTO documento { get; set; }
       public DocumentoRespuestaDTO pago { get; set; }
    }
}
