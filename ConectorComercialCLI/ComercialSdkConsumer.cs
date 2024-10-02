using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ConectorComercialCLI.Comercial;
using Microsoft.Win32;
using Newtonsoft.Json;
using ConectorComercialCLI.DTOs;
using System.Runtime.Remoting.Messaging;
using ConectorComercialCLI.DTOs.Existencias;
using System.Runtime.CompilerServices;
using ConectorComercialCLI.DTOs.Almacenes;
using ConectorComercialCLI.Sevices;
using static ConectorComercialCLI.DTOs.SdkDocTimbrar;
using System.Xml.Linq;

namespace ConectorComercialCLI
{

    public enum EComercialSdkConsumerMode
    {
        REGEDIT_CONFIG = 0,
        DIRECTLY_PARAMS = 1
    }

    /*
     * clase donde se definen todos los metodos que consumen el
     * sdk de comercial
    */
    class ComercialSdkConsumer
    {
        //

        EComercialSdkConsumerMode mode = EComercialSdkConsumerMode.REGEDIT_CONFIG;
        private RegistryKey gKeySistema;
        private String gRutaRegEditComercialPremium;
        private String gNombreDelSistema;
        private String gRutaInstalacion;
        //[System.Runtime.InteropServices.DllImport("KERNEL32")]
        //public static extern int SetCurrentDirectory(string pPtrDirActual);
        long gResultado;
        private int gError;
        private string serializedMesssage;

        // contpaq
        private tDocumento doc;
        private int idDocumento;
        private int idMovimiento;
        //  tMovimiento es utilizado cuando ingresamos movimientos sin descuentos
        private tMovimiento Mov;
        //  tMovimientoDesc es utilizado cuando ingresamos movimientos con descuentos
        private tMovimientoDesc MovDesc;
        private tSeriesCapas slp;
        private int i;
        private Random rnd;
        private int aleatorio;


        public ComercialSdkConsumer()
        {

        }

        public ComercialSdkConsumer(ComercialSdkConsumerConfigDTO config)
        {
            this.mode = config.mode;
        }

        public void inicializaSdk()
        {
            try
            {

                this.gRutaRegEditComercialPremium = @"SOFTWARE\WOW6432Node\Computación en Acción, SA CV\CONTPAQ I COMERCIAL";
                this.gKeySistema = Registry.LocalMachine.OpenSubKey(this.gRutaRegEditComercialPremium);
                this.gRutaInstalacion = gKeySistema.GetValue("DirectorioBase").ToString();

                this.gNombreDelSistema = "CONTPAQ I Comercial";
                //this.gResultado = Directory.SetCurrentDirectory(this.gRutaInstalacion);                
                Directory.SetCurrentDirectory(this.gRutaInstalacion);
                Environment.CurrentDirectory = this.gRutaInstalacion;

                ComercialSdk.fInicioSesionSDK(App.config.sdkUser, App.config.sdkPass);
                this.gError = Comercial.ComercialSdk.fSetNombrePAQ(this.gNombreDelSistema);

                if (this.gError != 0)
                {
                    this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK");
                    Console.WriteLine(this.serializedMesssage);
                    Environment.Exit(0);
                }
                else
                {
                    //  09.-    Si hasta este momento ha sido correcto la conexión, es hora de Inicializar SDK
                    this.gError = Comercial.ComercialSdk.fInicializaSDK();
                    //  10.-    Validar que el SDK no genere Errores.
                    if (this.gError != 0)
                    {
                        this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK");
                        Console.WriteLine(this.serializedMesssage);
                        Environment.Exit(0);
                    }
                    else
                    {
                        ComercialSdk.fInicioSesionSDKCONTPAQi(App.config.sdkUser, App.config.sdkPass);
                        //ComercialSdk.fInicioSesionSDK(App.config.sdkUser, App.config.sdkPass);
                        //Console.WriteLine("inicializaSdk pass");
                    }

                }
            }
            catch (Exception ex)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, ex.Message, "App");
                Console.WriteLine(this.serializedMesssage);
                Environment.Exit(0);
            }
        }

        public void abreEmpresa(string empresaRutaDatos)
        {
            try
            {
                this.gError = ComercialSdk.fAbreEmpresa(empresaRutaDatos);
                if (this.gError != 0)
                {
                    this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", "abre empresa");
                    Console.WriteLine(this.serializedMesssage);
                }
                //Console.WriteLine("abreEmpresa pass");
            }
            catch (Exception e)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App");
                Console.WriteLine(this.serializedMesssage);
                Environment.Exit(0);
            }
        }

        public void crearCliente(tCteProv cliente)
        {
            int aIdCteProv = 0;
            //tCteProv cliente = new tCteProv();
            //cliente.cCodigoCliente = "CLI1000";
            //cliente.cRFC = "XAXX010101XXX";
            //cliente.cRazonSocial = "CLIENTE PRUEBA S.A. DE C.V.";
            //cliente.cTipoCliente = 1;
            this.gError = ComercialSdk.fAltaCteProv(ref aIdCteProv, ref cliente);
            if (this.gError != 0)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", "crea cliente");
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
            else
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(true, "Cliente guardado exitosamente", cliente);
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
        }

        public void creaUnidadMedida(tUnidad unidad)
        {
            int aIdUnidad = 0;
            this.gError = ComercialSdk.fAltaUnidad(ref aIdUnidad, ref unidad);
            if (this.gError != 0)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", "crea unidad medida");
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
            else
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(true, "Unidad de medida guardada exitosamente", unidad);
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
        }

        public void streamExistenciasProducto(string codigoProducto)
        {
            DateTime now = DateTime.Now;
            double aExistencia = 0;
            this.gError = ComercialSdk.fRegresaExistencia(codigoProducto, "1", $"{now.Year}", $"{now.Month}", $"{now.Day}", ref aExistencia);
            if (this.gError != 0)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", "consultando existencias del producto " + codigoProducto);
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
            else
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(true, "Existencias del producto consultadas", aExistencia);
                Console.WriteLine(this.serializedMesssage);
                this.finalizaSdk();
                Environment.Exit(0);
            }
        }


        public void timbrarDocumento(Comprobante data)
        {
            int intentos = 0;
            int maxIntentos = 5;
            bool exito = false;

            while (intentos < maxIntentos && !exito)
            {
                this.gError = ComercialSdk.fEmitirDocumento($"{data.aCodConcepto}", $"{data.aSerie}", double.Parse(data.aFolio), $"{data.aPassword}", "");

                if (this.gError == 0)
                {
                    // Si el timbrado fue exitoso
                    exito = true;
                    App.logs.add($@"Documento Timbrado {data.aSerie} - {data.aFolio}");
                    //App.logs.add(true, "Documento timbrado exitosamente", data.aFolio);
                    Console.WriteLine(this.serializedMesssage);
                }
                else
                {
                    // Si hubo un error
                    intentos++;
                    App.logs.add($"Intento {intentos}: Error al timbrar el documento - {this.ErrorComercial(this.gError)}");

                    if (intentos >= maxIntentos)
                    {
                        // Después de 3 intentos, si no se logra timbrar, se considera como error
                        this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", $"Error al timbrar el documento después de {intentos} intentos.");
                        Console.WriteLine(this.serializedMesssage);
                        this.finalizaSdk();
                        Environment.Exit(0);
                    }
                }
            }

            // Finaliza el SDK si se logró el timbrado o si se intentó 3 veces sin éxito
            this.finalizaSdk();
        }

        public ConsultaExistenciasProductoRespuestaDto getExistenciasProducto(ConsultaExistenciasProductoDto data)
        {
            DateTime now = DateTime.Now;
            double aExistencia = 0;
            string codAlmacen = data?.codAlmacen != null ? data.codAlmacen : null;
            AlmacenesService almacenesService = new AlmacenesService();
            ConsultaExistenciasProductoRespuestaDto existenciasRespuesta = new ConsultaExistenciasProductoRespuestaDto();
            existenciasRespuesta.existencias = new List<ExistenciasAlmacenDto>();
            existenciasRespuesta.codProducto = data.codProducto;

            // se consultan los almacenes en base a los parametros de busqueda
            ConsultaAlmacenesRespuestaDto almacenesResult = almacenesService.getAlmacenes(new ConsultaAlmacenesDto()
            {
                codAlmacen = codAlmacen,
                codProducto = data.codProducto
            });

            if (almacenesResult.error == null)
            {

                foreach (AlmacenDto almacen in almacenesResult.almacenes)
                {
                    this.gError = ComercialSdk.fRegresaExistencia(data.codProducto, almacen.CCODIGOALMACEN, $"{now.Year}", $"{now.Month}", $"{now.Day}", ref aExistencia);
                    if (this.gError != 0)
                    {
                        // se agrega la respuesta con error
                        existenciasRespuesta.existencias.Add(new ExistenciasAlmacenDto()
                        {
                            codAlmacen = almacen.CCODIGOALMACEN,
                            existencias = aExistencia,
                            error = new ErrorDTO()
                            {
                                code = this.gError,
                                message = this.ErrorComercial(this.gError)
                            }
                        });
                    }
                    else
                    {
                        // se agrega la existencia a la respuesta
                        existenciasRespuesta.existencias.Add(new ExistenciasAlmacenDto()
                        {
                            codAlmacen = almacen.CCODIGOALMACEN,
                            existencias = aExistencia,
                            error = null
                        });
                    }
                }
            }

            return existenciasRespuesta;
        }


        public void creaProducto(tProducto producto)
        {
            int aIdProducto = 0;
            this.gError = ComercialSdk.fAltaProducto(ref aIdProducto, ref producto);
            if (this.gError != 0)
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(false, this.ErrorComercial(this.gError), "CONTPAQ SDK", "crea producto");
                Console.WriteLine(this.serializedMesssage);
                //this.finalizaSdk();
                //Environment.Exit(0);
            }
            else
            {
                this.serializedMesssage = App.responseCLI.serializeMessage(true, "Producto guardado exitosamente", producto, aIdProducto.ToString());
                Console.WriteLine(this.serializedMesssage);
                //this.finalizaSdk();
                //Environment.Exit(0);
            }
        }

        public DTOs.DocumentoRespuestaDTO creaDocumento(DTOs.DocumentoRespuestaDTO documento)
        {
            int idDocumento = 0;
            tDocumento doc = new tDocumento();
            doc.aAfecta = 0;
            doc.aSistemaOrigen = 5;
            documento.importe = documento.importe.Replace("$", "").Replace(",", "").Replace("-", "");
            if (documento.codConcepto != "")
            {
                doc.aCodConcepto = documento.codConcepto;
            }
            if (documento.codigoAgente != "" && documento.codigoAgente != null)
            {
                doc.aCodigoAgente = documento.codigoAgente;
            }
            if (documento.codigoCteProv != "")
            {
                doc.aCodigoCteProv = documento.codigoCteProv;
            }
            if (documento.descuentoDoc1 != "" && documento.descuentoDoc1 != "0" && documento.descuentoDoc1 != null)
            {
                doc.aDescuentoDoc1 = double.Parse(documento.descuentoDoc1);
            }
            if (documento.fecha != "")
            {
                // format: yyyy-MM-ddTHH:mm:ss -- ENTRADA DEL CLI
                doc.aFecha = DateTime.Parse(documento.fecha).ToString("MM/dd/yyyy");
            }
            if (documento.folio != "")
            {
                doc.aFolio = int.Parse(documento.folio);
            }
            if (documento.importe != "" && documento.importe != "0" && documento.importe != null)
            {
                doc.aImporte = double.Parse(documento.importe);
            }
            if (documento.numMoneda != "")
            {
                doc.aNumMoneda = int.Parse(documento.numMoneda);
            }
            if (documento.referencia != "")
            {
                doc.aReferencia = documento.referencia;
            }
            if (documento.serie != "")
            {
                doc.aSerie = documento.serie;
            }
            if (documento.tipoCambio != "")
            {
                doc.aTipoCambio = double.Parse(documento.tipoCambio);
            }

            this.gError = ComercialSdk.fAltaDocumento(ref idDocumento, ref doc);

            if (this.gError != 0)
            {
                App.logs.add(this.ErrorComercial(this.gError));
                documento.error = new DTOs.ErrorDTO
                {
                    message = this.ErrorComercial(this.gError),
                    code = this.gError
                };
            }
            else
            {
                documento.id = idDocumento.ToString();
                documento.folio = doc.aFolio.ToString();
                documento.importe = doc.aImporte.ToString();

                App.logs.add($"Documento con folio {idDocumento} generado");
                int consecutivo = 1;
                foreach (DTOs.MovimientoDocumentoDTO movimiento in (documento?.movimientos ?? new MovimientoDocumentoDTO[0]))
                {
                    int idMovimiento = 0;
                    movimiento.precio = movimiento.precio.Replace("$", "").Replace(",", "").Replace("-", "");
                    tMovimiento mov = new tMovimiento();
                    mov.aConsecutivo = consecutivo;
                    if (movimiento.unidades != "")
                    {
                        mov.aUnidades = double.Parse(movimiento.unidades);
                    }
                    if (movimiento.precio != "")
                    {
                        mov.aPrecio = double.Parse(movimiento.precio);
                    }
                    if (movimiento.costo != "" && movimiento.costo != null)
                    {
                        mov.aCosto = double.Parse(movimiento.costo);
                    }
                    if (movimiento.codProdSer != "")
                    {
                        mov.aCodProdSer = movimiento.codProdSer;
                    }
                    if (movimiento.codAlmacen != "" && movimiento.codAlmacen != null)
                    {
                        mov.aCodAlmacen = movimiento.codAlmacen;
                    }
                    if (movimiento.referencia != "" && movimiento.referencia != null)
                    {
                        mov.aReferencia = movimiento.referencia;
                    }


                    this.gError = ComercialSdk.fAltaMovimiento(idDocumento, ref idMovimiento, ref mov);

                    if (this.gError != 0)
                    {
                        App.logs.add($"Error al agregar el movimiento {movimiento.consecutivo}{Environment.NewLine}{this.ErrorComercial(this.gError)}");
                    }
                    else
                    {
                        movimiento.consecutivo = mov.aConsecutivo.ToString();
                        movimiento.id = idMovimiento.ToString();
                        movimiento.ctextoExtra1 = movimiento.ctextoExtra1;
                        movimiento.codProdSer = mov.aCodProdSer;
                        movimiento.precio = mov.aPrecio.ToString();
                        consecutivo++;
                        App.logs.add($"Movimiento {movimiento.consecutivo} agregado al documento {idDocumento}");
                    }
                }
            }
            return documento;


        }

        public EliminarDocumentoRespuestaDTO eliminarDocumento(tLlaveDoc llaveDoc)
        {
            EliminarDocumentoRespuestaDTO respuesta = new EliminarDocumentoRespuestaDTO(llaveDoc);
            try
            {
                // se busca el documento de comercial con el sdk
                this.gError = ComercialSdk.fBuscaDocumento(ref llaveDoc);

                if (this.gError != 0)
                {
                    App.logs.add($"Error al eliminar el documento ({this.ErrorComercial(this.gError)})");
                    respuesta.error = new ErrorDTO() { code = this.gError, message = this.ErrorComercial(this.gError) };
                }
                else
                {
                    this.gError = ComercialSdk.fBorraDocumento();
                    if (this.gError != 0)
                    {
                        App.logs.add($"Error al eliminar el documento ({this.ErrorComercial(this.gError)})");
                        respuesta.error = new ErrorDTO() { code = this.gError, message = this.ErrorComercial(this.gError) };
                    }
                }
            }
            catch (Exception e)
            {
                App.logs.add($"[Error] Error al eliminar el documento: {e.StackTrace}");
                respuesta.error = new ErrorDTO() { code = this.gError, message = "Error al eliminar el documento" };
                Console.WriteLine(e.StackTrace);
            }
            return respuesta;
        }

        public AplicandoPagoDocumentoDTO saldarDocumento(AplicandoPagoDocumentoDTO aplicandoPago)
        {
            try
            {
                tLlaveDoc llaveDocumento = new tLlaveDoc();
                llaveDocumento.aSerie = aplicandoPago.documento.serie;
                llaveDocumento.aFolio = Convert.ToDouble(aplicandoPago.documento.folio);
                llaveDocumento.aCodConcepto = aplicandoPago.documento.codConcepto;

                tLlaveDoc llavePago = new tLlaveDoc();
                llavePago.aSerie = aplicandoPago.pago.serie;
                llavePago.aFolio = Convert.ToDouble(aplicandoPago.pago.folio);
                llavePago.aCodConcepto = aplicandoPago.pago.codConcepto;

                // consultar el id de la moneda

                this.gError = ComercialSdk.fSaldarDocumento(
                    ref llaveDocumento,
                    ref llavePago,
                    Convert.ToDouble(aplicandoPago.pago.importe),
                    Convert.ToInt32(aplicandoPago.pago.numMoneda),
                    DateTime.Now.ToString("MM/dd/yyyy")
                );
                if (this.gError != 0)
                {
                    App.logs.add($"Error al saldar el documento ({this.ErrorComercial(this.gError)})");
                    aplicandoPago.error = new ErrorDTO() { code = this.gError, message = this.ErrorComercial(this.gError) };
                    return aplicandoPago;
                }
                else
                {
                    return aplicandoPago;
                }
            }
            catch (Exception e)
            {
                App.logs.add($"[Error] Error al saldar el documento: {e.StackTrace}");
                aplicandoPago.error = new ErrorDTO() { code = this.gError, message = "Error al saldar el documento" };
                Console.WriteLine(e.StackTrace);
                return aplicandoPago;
            }
        }

        public int saldarDocumentoPago(tLlaveDoc Factura, tLlaveDoc DocPago, double Importe, int Moneda, string FechaPago)
        {
            int Bandera = 0; //0 significa Fallo, 1 Se saldo
            try
            {
                this.gError = ComercialSdk.fSaldarDocumento(
                    ref Factura,
                    ref DocPago,
                    Convert.ToDouble(Importe),
                    Convert.ToInt32(Moneda),
                    FechaPago
                );
                if (this.gError != 0)
                {
                    App.logs.add($"Error al saldar el documento ({this.ErrorComercial(this.gError)})");
                    Console.WriteLine(this.ErrorComercial(this.gError));
                    return Bandera;
                }
                else
                {
                    Bandera = 1;
                    return Bandera;
                }
            }
            catch (Exception e)
            {
                App.logs.add($"[Error] Error al saldar el documento: {e.StackTrace}");
                return 0;
            }
        }

        public string ErrorComercial(int pNumberError)
        {
            //  Construcción de string con logitud de 512 Caracteres
            StringBuilder lMensaje = new StringBuilder(512);
            //  Mandamos a llamar funcion de Erro
            //  pNumeroError    ->  Codigo del error, debe ser diferente de 0.
            //  lMensaje        ->  Retorno del mensaje de error.
            //  512             ->  Longitud de caracteres.
            ComercialSdk.fError(pNumberError, lMensaje, 512);
            return lMensaje.ToString();
        }

        public void finalizaSdk()
        {
            Comercial.ComercialSdk.fCierraEmpresa();
            Comercial.ComercialSdk.fTerminaSDK();
        }
    }
}
