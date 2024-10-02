using ConectorComercialCLI.Comercial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class EliminarDocumentoRespuestaDTO
    {
        public string aCodConcepto;
        public string aSerie;
        public double aFolio;

        public EliminarDocumentoRespuestaDTO(tLlaveDoc llaveDoc)
        {
            this.aCodConcepto = llaveDoc.aCodConcepto;
            this.aFolio = llaveDoc.aFolio;
            this.aSerie= llaveDoc.aSerie;
        }

        //error
        public ErrorDTO error { get; set; }
    }
}
