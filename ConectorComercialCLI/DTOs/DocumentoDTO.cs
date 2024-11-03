using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConectorComercialCLI.DTOs
{
    public class DocumentoDTO
    {
        public string id { get; set; } = null;
        public string folio { get; set; }
        public string numMoneda { get; set; }
        public string tipoCambio { get; set; }
        public string importe { get; set; }
        public string descuentoDoc1 { get; set; }
        public string codConcepto { get; set; }
        public string serie { get; set; }
        public string fecha { get; set; } // format: yyyy-MM-ddTHH:mm:ss
        public string codigoCteProv { get; set; }
        public string codigoAgente { get; set; }
        public string referencia { get; set; }
        public string observaciones { get; set; }
        public bool TimbrarCFDI { get; set; }
        public string LugarExp { get; set; }
        public string FormaPago { get; set; }
        public string CondicionPago { get; set; }
        public string UsoCFDI { get; set; }
        public string MetodoPago { get; set; }
        public string RegimenFiscal { get; set; }
        public string extraData {  get; set; }
        public string FileName { get; set; }

        public string Procesado { get; set; }
        public MovimientoDocumentoDTO[] movimientos { get; set; }
        public DocumentoRelacionadoDTO[] docRelacionados { get; set; } = new DocumentoRelacionadoDTO[0];
        public DocRelacionadosNC[] docRelacionadosNCs { get; set; } = new DocRelacionadosNC[0];


    }
    public class DocumentoRelacionadoDTO
    {
        public string UUID { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string MonedaDR { get; set; }
        public double NumParcialidad { get; set; }
        public double ImpPagado { get; set; }
        public string ObjetoImpDR { get; set; }
        public double ImpSaldoAnt { get; set; }
        public string ConceptoDocumento { get; set; } = string.Empty;
    }

    public class DocRelacionadosNC
    {
        public string UUID { get; set; }
        public string Serie { get; set; }
        public string Folio { get; set; }
        public string ConceptoDocumento { get; set; } = string.Empty;
        public string TipoRelacion {  get; set; }   
    }

}
