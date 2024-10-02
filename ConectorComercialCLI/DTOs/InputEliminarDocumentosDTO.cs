using ConectorComercialCLI.Comercial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class InputEliminarDocumentosDTO : BaseInput
    {
        public tLlaveDoc[] documentos { get; set; }
    }
}
