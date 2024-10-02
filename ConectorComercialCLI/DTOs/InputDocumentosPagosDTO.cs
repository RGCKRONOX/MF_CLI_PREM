using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class InputDocumentosPagosDTO : BaseInput
    {
        public AplicandoPagoDocumentoDTO[] docs { get; set; }
    }
}
