using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class InputDocumentosDTO : BaseInput
    {
        public DocumentoDTO[] documentos { get; set; }
    }
}
