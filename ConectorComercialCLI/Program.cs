using System;
using System.Data.Common;
using CommandLine;
using Newtonsoft.Json;
using Kronox.Util;
using System.Collections.Generic;
using System.IO;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using System.Data.Sql;
using ConectorComercialCLI.DTOs;
using System.Data.Odbc;
using System.Runtime.InteropServices;
using System.Linq;
using ConectorComercialCLI.Comercial;
using System.ComponentModel;
using ConectorComercialCLI.DTOs.Existencias;
using static ConectorComercialCLI.DTOs.SdkDocTimbrar;
using System.Xml.Linq;
using System.Runtime.Remoting.Messaging;

namespace ConectorComercialCLI
{

    /*
     * 
     * configuracion del conector
     * estos parametros se guardan y se consultan del registro de windows
     * con la ruta SOFTWARE\KronoxYKairos\ConectorComercialCLI en la llave HKEY_CURRENT_USER
     * 
    */
    public class DocumentoDTONC
    {
        public int CIDDOCUMENTO { get; set; }
        public string CSERIEDOCUMENTO { get; set; }
        public string CFOLIO { get; set; }
        public string CIDCONCEPTODOCUMENTO { get; set; }
        public decimal CPENDIENTE { get; set; }
        public string CUUID { get; set; } 
        public decimal CABONO { get; set; }
    }

    class ConectorConfig
    {
        // config SQL SERVER
        public string dbInstance { get; set; }
        public string dbDatabase { get; set; }
        public string dbUser { get; set; }
        public string dbPass { get; set; }

        // config CONTPAQ
        public string sdkUser { get; set; }
        public string sdkPass { get; set; }
        public string sdkEmpresaEnUso { get; set; }


        // logs
        public string logDir { get; set; }

        // app dirs
        public string appDir { get; set; }
    }

    /*
      * Objeto Global para acceder desde cualquier clase del proyecto
      * este objeto contiene manejadores de registro, manejadores de mensajes del conector , etc
    */
    static class App
    {
        public static AppMessages responseCLI;
        public static ComercialSdkConsumer sdkConsumer;
        public static Registro regManager;
        public static ConectorConfig config;
        public static AppLogs logs;

        public static void getConectorConfig()
        {
            /*
             *   se inicializan los valores en el registro en caso de que no exista la ruta
             *   para evitar que arroje excepciones
            */
            App.regManager = new Registro(@"SOFTWARE\KronoxYKairos\ConectorComercialCLI", "currenUser", new Dictionary<string, string>()
            {
                {"dbInstance", ""},
                {"dbDatabase", ""},
                {"dbUser", ""},
                {"dbPass", ""},
                {"sdkUser", ""},
                {"sdkPass", ""},
                {"sdkEmpresaEnUso", ""},
                {"llavesConfigExtraContpaq", ""},
                {"appDir", Directory.GetCurrentDirectory()},
                {"logDir", $@"{Directory.GetCurrentDirectory()}\logs" }
            });

            // se inicializa la configuracion del conector
            App.config = new ConectorConfig()
            {
                dbInstance = App.regManager.ObtenerValorDeClave("dbInstance"),
                dbDatabase = App.regManager.ObtenerValorDeClave("dbDatabase"),
                dbUser = App.regManager.ObtenerValorDeClave("dbUser"),
                dbPass = App.regManager.ObtenerValorDeClave("dbPass"),
                sdkUser = App.regManager.ObtenerValorDeClave("sdkUser"),
                sdkPass = App.regManager.ObtenerValorDeClave("sdkPass"),
                sdkEmpresaEnUso = App.regManager.ObtenerValorDeClave("sdkEmpresaEnUso"),
                logDir = App.regManager.ObtenerValorDeClave("logDir"),
                appDir = App.regManager.ObtenerValorDeClave("appDir"),
                //archivoLicencia = Globales.regManager.ObtenerValorDeClave("AppKey"),
            };


        }
    }


    class Program
    {
        /*
         * Esta clase define la estructura base de los comandos del conector
         */
        public class Options
        {
            // accion a realizar en el conector
            [Option('a', "accion", Required = true, HelpText = "Acción necesaria  (-a [insertar | consultar | crear])")]
            public string accion { get; set; }

            // recurso sobre el cual se ejecutara la accion
            [Option('r', "recurso", Required = true, HelpText = "Recurso necesario  (-r [productos | clientes | unidades | documentos | configuracion-contpaq | configuracion-sqlserver])")]
            public string recurso { get; set; }

            // archivo de datos que es requerido cuando se utiliza la accion "insertar"
            [Option('f', "archivo", Required = false, HelpText = "Ruta del archivo de datos o dato para el recurso")]
            public string archivo { get; set; }


            // si se inicia en modo REGEDIT_CONFIG se tomara la conffiguracion que se guarde con el comando "inserta configuracion-contpaq" e "inserta configuracion-sqlserver"
            // si se inicia en modo DIRECTLY_CONFIG se le pasara la configuracion al conector en el mismo archivo de datos (solo comandos con accion "crear")
            // archivo de datos que es requerido cuando se utiliza la accion "insertar" 
            [Option('m', "mode", Required = false, HelpText = "Modo en el que va a estar trabajando el conector ( REGEDIT_CONFIG | DIRECTLY_CONFIG )")]
            public string mode { get; set; }
        }

        static void Main(string[] args)
        {
            // se inicializa el manejador de mensajes de respuesta del conector
            App.responseCLI = new AppMessages();

            // se incializa el manejador de logs del conector
            App.logs = new AppLogs($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\ConectorComercialCLI");


            App.getConectorConfig();
            App.sdkConsumer = new ComercialSdkConsumer();
            //crearDocumentNC("C:\\Kronox\\MF_CLI_PREM\\CLI.json");
            //saldarDocumentos("C:\\Desarrollo\\Mfranklin\\CLI.json");


            // funcion para leer el comando de entrada y leer los parametros del comando
            Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.mode == "REGEDIT_CONFIG" || o.mode == null)
                       {
                           App.getConectorConfig();
                           App.sdkConsumer = new ComercialSdkConsumer();
                       }


                       /*
                        * se definen las acciones y recursos disponibles para el conector
                       */

                       if (o.accion == "insertar" && o.recurso == "productos")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               insertaProducto(o.archivo);
                           }
                       }
                       else if (o.accion == "insertar" && o.recurso == "clientes")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               insertaCliente(o.archivo);
                           }
                       }
                       else if (o.accion == "insertar" && o.recurso == "unidades")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               insertaUnidadMedida(o.archivo);
                           }
                       }
                       else if (o.accion == "insertar" && o.recurso == "documento")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               insertaUnidadMedida(o.archivo);
                           }
                       }
                       else if (o.accion == "crear" && o.recurso == "documentos")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               crearDocumentos(o.archivo);
                           }
                       }
                       else if (o.accion == "crearNC" && o.recurso == "documentos")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               crearDocumentNC(o.archivo);
                           }
                       }
                       else if (o.accion == "aplicar" && o.recurso == "pagos")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               saldarDocumentos(o.archivo);
                           }
                       }
                       else if (o.accion == "insertar" && o.recurso == "configuracion-sqlserver")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               guardaConfiguracionSqlServer(o.archivo);
                           }
                       }
                       else if (o.accion == "consultar" && o.recurso == "configuracion-sqlserver")
                       {
                           consultarConfigSqlServer();
                       }
                       else if (o.accion == "insertar" && o.recurso == "configuracion-contpaq")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               guardaConfiguracionContpaq(o.archivo);
                           }
                       }
                       else if (o.accion == "insertar" && o.recurso == "documentos")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               sendFileRequiredMessage();
                           }
                           else
                           {
                               insertaDocumentos(o.archivo);
                           }
                       }
                       else if (o.accion == "consultar" && o.recurso == "configuracion-contpaq")
                       {
                           consultarConfigContpaq();
                       }
                       else if (o.accion == "consultar" && o.recurso == "empresas")
                       {
                           consultaEmpresas();
                       }
                       else if (o.accion == "consultar" && o.recurso == "instancias-sqlserver")
                       {
                           consultaInstanciasSqlServer();
                       }
                       else if (o.accion == "consultar" && o.recurso == "clientes")
                       {
                           consultaClientes();
                       } 
                       else if (o.accion == "consultar" && o.recurso == "productos_existencias")
                       {
                           consultarExistenciasProductos(o.archivo);
                       }
                       // productos
                       else if (o.accion == "consultar" && o.recurso == "producto_existencias")
                       {
                           if (o.archivo == "" || o.archivo == null)
                           {
                               Console.WriteLine(App.responseCLI.serializeMessage(false, "falta identificador del producto"));
                           }
                           else
                           {
                               getExistenciasProducto(o.archivo);
                           }
                       }
                       else
                       {
                           Console.WriteLine(App.responseCLI.serializeMessage(false, "Nada por hacer con la acción o el recurso especificado"));
                       }
                   });
        }

        // funcion para lanzar el mensaje de que se requere el archivo de datos
        // en caso de que no se le haya indicado al conector
        public static void sendFileRequiredMessage()
        {
            string serializedMesssage = App.responseCLI.serializeMessage(false, "La ruta del archivo de datos es requerido", "App", "Main");
            Console.WriteLine(serializedMesssage);
            Environment.Exit(0);
        }

        #region  acciones_contpaq

        /*
            se consultan las existencias con un codigo de producto
         */
        public static void getExistenciasProducto(string codigoProducto)
        {
            string serializedMesssage;

            if (codigoProducto != "")
            {
                App.sdkConsumer.inicializaSdk();
                App.sdkConsumer.abreEmpresa(App.config.sdkEmpresaEnUso);
                App.sdkConsumer.streamExistenciasProducto(codigoProducto);
            }
            else
            {
                serializedMesssage = App.responseCLI.serializeMessage(false, "No se pudo consultas las existencias del producto", "App", "Program::getExistenciasProducto");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }

        }

        /*
         * se inserta en contpaq un arreglo de productos de tipo tProducto[]
         * que se lee desde el archivo de datos indicado
         */
        public static void insertaProducto(string filePath)
        {
            FileManager fm = new FileManager();
            if (fm.checkIfExists(filePath))
            {
                string dataProducto = "";
                Comercial.tProducto[] productos = new Comercial.tProducto[0];
                string serializedMesssage;
                try
                {
                    dataProducto = fm.getContentFile(filePath, includePath: true);
                    productos = JsonConvert.DeserializeObject<Comercial.tProducto[]>(dataProducto);
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando producto");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (dataProducto != "")
                {
                    App.sdkConsumer.inicializaSdk();
                    App.sdkConsumer.abreEmpresa(App.config.sdkEmpresaEnUso);
                    foreach (Comercial.tProducto producto in productos)
                    {
                        App.sdkConsumer.creaProducto(producto);
                    }
                    App.sdkConsumer.finalizaSdk();
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de producto no válidos", "App", "insertando producto");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando producto");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }


        /*
        * se consultan los clientes desde SQL Server
        */
        public static void consultaClientes()
        {
            try
            {
                if (App.config.dbInstance != "")
                {
                    string[] rutaDatos = App.config.sdkEmpresaEnUso.Split('\\');
                    SQLSRV sqlManager = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                    {
                        instancia = App.config.dbInstance,
                        db = rutaDatos[rutaDatos.Length - 1],
                        usuario = App.config.dbUser,
                        contrasena = App.config.dbPass
                    });
                    List<DTOs.ClienteDTO> clientes = new List<DTOs.ClienteDTO>();
                    sqlManager.Conecta();
                    SqlDataReader reader = sqlManager.EjecutarLecturaDeDatos("select CCODIGOCLIENTE as codigo from admClientes");
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            clientes.Add(new DTOs.ClienteDTO()
                            {
                                codigo = reader["codigo"].ToString()
                            }); ;
                        }
                    sqlManager.Desconecta();

                    string serializedMesssage = App.responseCLI.serializeMessage(true, "clientes consultados", clientes);
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
                else
                {
                    string serializedMesssage = App.responseCLI.serializeMessage(false, "configuracion requerida", "App", "consultando clientes");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "consultando clientes");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }

        /*
        * se inserta en contpaq un arreglo de cliente de tipo tCliente
        * que se lee desde el archivo de datos indicado
        */
        public static void insertaCliente(string filePath)
        {
            FileManager fm = new FileManager();
            if (fm.checkIfExists(filePath))
            {
                string dataCliente = "";
                Comercial.tCteProv cliente = new Comercial.tCteProv();
                string serializedMesssage;
                try
                {
                    dataCliente = fm.getContentFile(filePath, includePath: true);
                    cliente = JsonConvert.DeserializeObject<Comercial.tCteProv>(dataCliente);
                    //Console.WriteLine(cliente.cRFC);

                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando cliente");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (dataCliente != "")
                {
                    App.sdkConsumer.inicializaSdk();
                    App.sdkConsumer.abreEmpresa(App.config.sdkEmpresaEnUso);
                    App.sdkConsumer.crearCliente(cliente);
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de cliente no válidos", "App", "insertando cliente");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando cliente");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }

        /*
        * se inserta en contpaq un arreglo de unidades de media de tipo tUnidad
        * que se lee desde el archivo de datos indicado
        */
        public static void insertaUnidadMedida(string filePath)
        {
            FileManager fm = new FileManager();
            if (fm.checkIfExists(filePath))
            {
                string dataUnidad = "";
                Comercial.tUnidad unidad = new Comercial.tUnidad();
                string serializedMesssage;
                try
                {
                    dataUnidad = fm.getContentFile(filePath, includePath: true);
                    unidad = JsonConvert.DeserializeObject<Comercial.tUnidad>(dataUnidad);
                    //Console.WriteLine(cliente.cRFC);

                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando unidad");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (dataUnidad != "")
                {
                    App.sdkConsumer.inicializaSdk();
                    App.sdkConsumer.abreEmpresa(App.config.sdkEmpresaEnUso);
                    App.sdkConsumer.creaUnidadMedida(unidad);
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de la unidad no válidos", "App", "insertando unidad");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando unidad");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }

        /*
        * se inserta en contpaq un arreglo de documentos de tipo DocumentoDTO[]
        * que se lee desde el archivo de datos indicado
        */
        public static void insertaDocumentos(string filePath)
        {
            FileManager fm = new FileManager();
            if (fm.checkIfExists(filePath))
            {
                string dataDocumentos = "";
                DTOs.DocumentoDTO[] documentos = new DTOs.DocumentoDTO[0];
                string serializedMesssage;
                try
                {
                    dataDocumentos = fm.getContentFile(filePath, includePath: true);
                    documentos = JsonConvert.DeserializeObject<DTOs.DocumentoDTO[]>(dataDocumentos);
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando documentos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (dataDocumentos != "")
                {
                    App.sdkConsumer.inicializaSdk();
                    App.sdkConsumer.abreEmpresa(App.config.sdkEmpresaEnUso);
                    List<DTOs.DocumentoDTO> documentosRespuesta = new List<DTOs.DocumentoDTO>();
                    foreach (DTOs.DocumentoDTO documento in documentos)
                    {
                        var serializedParent = JsonConvert.SerializeObject(documento);
                        DTOs.DocumentoRespuestaDTO docTemp = JsonConvert.DeserializeObject<DTOs.DocumentoRespuestaDTO>(serializedParent);
                        //crear documento
                        documentosRespuesta.Add(App.sdkConsumer.creaDocumento(docTemp));
                    }
                    App.sdkConsumer.finalizaSdk();
                    serializedMesssage = App.responseCLI.serializeMessage(true, "Documentos procesados", documentosRespuesta.ToArray());
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de documentos no válidos", "App", "insertando documentos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando documentos");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }

        

        public static void crearDocumentos(string fileData)
        {
            FileManager fm = new FileManager();
            App.logs.add($@"*****************************************************************");
            App.logs.add($@"leyendo archivo {fileData}");
            if (fm.checkIfExists(fileData))
            {
                string data = "";
                string serializedMesssage;
                InputDocumentosDTO input = new InputDocumentosDTO();
                try
                {
                    data = fm.getContentFile(fileData, includePath: true);
                    input = JsonConvert.DeserializeObject<InputDocumentosDTO>(data);

                    App.config = new ConectorConfig()
                    {
                        dbInstance = input.instancia,
                        dbDatabase = input.empresaDB,
                        dbUser = input.userSQL,
                        dbPass = input.passSQL,
                        sdkUser = input.userSDK,
                        sdkPass = input.passSDK,
                        sdkEmpresaEnUso = input.rutaEmpresa
                    };
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando documentos");
                    App.logs.add($@"error al convertir los datos de documentos {e.StackTrace}");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (data != "")
                {
                    try
                    {
                        ComercialSdkConsumer sdk = new ComercialSdkConsumer(new ComercialSdkConsumerConfigDTO()
                        {
                            userSDK = input.userSDK,
                            passSDK = input.passSDK
                        });
                        sdk.inicializaSdk();
                        sdk.abreEmpresa(input.rutaEmpresa);
                        SQLSRV db = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                        {
                            instancia = input.instancia,
                            db = input.empresaDB,
                            usuario = input.userSQL,
                            contrasena = input.passSQL
                        });
                        db.Conecta();
                        // se acumulan los documentos que se estan procesando para retornar una respuesta
                        List<DocumentoDTO> documentosRespuesta = new List<DocumentoDTO>();
                        foreach (DocumentoDTO documento in input.documentos)
                        {
                            var serializedParent = JsonConvert.SerializeObject(documento);
                            DocumentoRespuestaDTO docTemp = JsonConvert.DeserializeObject<DocumentoRespuestaDTO>(serializedParent);
                            //crear documento
                            string valor;

                            //Validar que solamente facturas que no este el prodcuto en catalogo no se suba. 
                            string resultadoConcepto = db.ExecuteScalar($"SELECT CIDDOCUMENTODE FROM admConceptos WHERE CCODIGOCONCEPTO = '{documento.codConcepto}' AND CESTATUS = 1 ");
                            if (resultadoConcepto  == "4")
                            {
                                int totalMovimientos = docTemp.movimientos.Length; // Para un array

                                int productosEncontrados = 0;
                                //Validare si todos los productos existen
                                foreach (MovimientoDocumentoDTO movimiento in docTemp.movimientos)
                                {
                                    string resultado = db.ExecuteScalar($"SELECT * FROM admProductos WHERE CCODIGOPRODUCTO = '{movimiento.codProdSer}' AND CSTATUSPRODUCTO = 1");

                                    if (resultado != null && Convert.ToInt32(resultado) > 0)
                                    {
                                        productosEncontrados++; // Incrementa el contador si se encuentra el producto
                                    }
                                    else
                                    {
                                        App.logs.add($@"Error: No se encuentra el producto {movimiento.codProdSer}");
                                    }
                                }
                                valor = totalMovimientos == productosEncontrados ? "" : "1";
                                documento.Procesado = valor;
                            }

                            if (documento.Procesado == "")
                            {
                                DocumentoRespuestaDTO docRespuesta = sdk.creaDocumento(docTemp);
                                docRespuesta.FileName = docTemp.FileName;
                                if (docRespuesta.error == null)
                                {
                                    // se consulta el folio insertado y se valida si es que se inserto
                                    if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docId) && docId > 0)
                                    {
                                        docRespuesta.folio = db.ExecuteScalar($"SELECT CFOLIO FROM admDocumentos WHERE CIDDOCUMENTO = {docRespuesta.id}");
                                    }

                                    documentosRespuesta.Add(docRespuesta);
                                    // se valida si el documento se ha creado para setear el dato de observaciones
                                    if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docIde) && docIde > 0)
                                    {
                                        int affectedRows = db.EjecutaSimpleQuery($"" +
                                            $"UPDATE admDocumentos SET COBSERVACIONES = '{documento.observaciones}', " +
                                            $"CCANTPARCI = '{documento.MetodoPago}',  CLUGAREXPE = '{documento.LugarExp}' , CMETODOPAG= '{documento.FormaPago}'," +
                                            $"CCONDIPAGO = '{documento.CondicionPago}' WHERE CIDDOCUMENTO = {docRespuesta.id}");

                                        int affectedRowsFolios = db.EjecutaSimpleQuery($"UPDATE admFoliosDigitales SET CCODCONCBA = '{documento.UsoCFDI}' , CDESCAUT03 ='{documento.RegimenFiscal}'  WHERE CIDDOCTO = {docRespuesta.id}");
                                        if (affectedRows <= 0)
                                        {
                                            App.logs.add($@"Error: No se pudo actualizar las observaciones del documento {docRespuesta.folio}");
                                        }
                                        foreach (MovimientoDocumentoDTO movimiento in docRespuesta.movimientos)
                                        {
                                            int affectedMovimientos = db.EjecutaSimpleQuery($"UPDATE ADMMOVIMIENTOS SET CTEXTOEXTRA1 = '{movimiento.ctextoExtra1}' WHERE CIDMOVIMIENTO = {movimiento.id}");
                                            if (affectedMovimientos <= 0)
                                            {
                                                App.logs.add($@"Error: No se pudo actualizar el campo CTEXTOEXTRA1 del Movimiento {docRespuesta.folio}");
                                            }
                                        }
                                    }
                                    if (documento.TimbrarCFDI)
                                    {
                                        Comprobante comprobante = new Comprobante();

                                        // Asignar valores a las propiedades del objeto
                                        comprobante.aSerie = documento.serie;
                                        comprobante.aCodConcepto = documento.codConcepto;
                                        comprobante.aFolio = documento.folio;
                                        comprobante.aPassword = input.PassCSD;
                                        sdk.timbrarDocumento(comprobante);
                                    }
                                }
                                else
                                {
                                    App.logs.add($@"{docRespuesta.error.message} - {documento.serie}- {documento.folio}");
                                }
                            }
                            

                        }
                        db.Desconecta();
                        sdk.finalizaSdk();
                        serializedMesssage = App.responseCLI.serializeMessage(true, "Documentos procesados", documentosRespuesta.ToArray());
                        App.logs.add($@"Documentos procesados");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);

                    }
                    catch (Exception e)
                    {
                        App.logs.add($"Error al procesar los documentos");
                        App.logs.add(e.StackTrace);
                    }
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de documentos no válidos", "App", "insertando documentos");
                    App.logs.add($@"datos de documentos no validos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando documentos");
                Console.WriteLine(serializedMesssage);
                App.logs.add($@"Archivo de datos no encontrado {fileData}");
                Environment.Exit(0);
            }
        }


        /**
         * se eliminan los documentos especificados en un arreglo 
         */
        public static void eliminarDocumentos(string fileData)
        {
            FileManager fm = new FileManager();
            App.logs.add($@"leyendo archivo {fileData}");
            if (fm.checkIfExists(fileData))
            {
                string data = "";
                string serializedMesssage;
                InputEliminarDocumentosDTO input = new InputEliminarDocumentosDTO();
                try
                {
                    data = fm.getContentFile(fileData, includePath: true);
                    input = JsonConvert.DeserializeObject<InputEliminarDocumentosDTO>(data);

                    App.config = new ConectorConfig()
                    {
                        dbInstance = input.instancia,
                        dbDatabase = input.empresaDB,
                        dbUser = input.userSQL,
                        dbPass = input.passSQL,
                        sdkUser = input.userSDK,
                        sdkPass = input.passSDK,
                        sdkEmpresaEnUso = input.rutaEmpresa
                    };
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "insertando documentos");
                    App.logs.add($@"error al convertir los datos de documentos {e.StackTrace}");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (data != "")
                {
                    try
                    {
                        ComercialSdkConsumer sdk = new ComercialSdkConsumer(new ComercialSdkConsumerConfigDTO()
                        {
                            userSDK = input.userSDK,
                            passSDK = input.passSDK
                        });
                        sdk.inicializaSdk();
                        sdk.abreEmpresa(input.rutaEmpresa);
                        SQLSRV db = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                        {
                            instancia = input.instancia,
                            db = input.empresaDB,
                            usuario = input.userSQL,
                            contrasena = input.passSQL
                        });
                        db.Conecta();
                        // se acumulan los documentos que se estan procesando para retornar una respuesta
                        List<EliminarDocumentoRespuestaDTO> documentosRespuesta = new List<EliminarDocumentoRespuestaDTO>();
                        foreach (tLlaveDoc documento in input.documentos)
                        {
                            EliminarDocumentoRespuestaDTO docRespuesta = sdk.eliminarDocumento(documento);
                            documentosRespuesta.Add(docRespuesta);
                        }
                        db.Desconecta();
                        sdk.finalizaSdk();
                        serializedMesssage = App.responseCLI.serializeMessage(true, "Documentos procesados", documentosRespuesta.ToArray());
                        App.logs.add($@"Documentos procesados");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);

                    }
                    catch (Exception e)
                    {
                        App.logs.add($"Error al procesar los documentos");
                        App.logs.add(e.StackTrace);
                    }
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de documentos no válidos", "App", "insertando documentos");
                    App.logs.add($@"datos de documentos no validos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "insertando documentos");
                Console.WriteLine(serializedMesssage);
                App.logs.add($@"Archivo de datos no encontrado {fileData}");
                Environment.Exit(0);
            }
        }

        public static void crearDocumentNC(string fileData)
        {
            FileManager fm = new FileManager();
            App.logs.add($@"leyendo archivo -Documento Pago {fileData}");
            if (fm.checkIfExists(fileData))
            {
                string data = "";
                string serializedMesssage;
                InputDocumentosDTO input = new InputDocumentosDTO();
                try
                {
                    data = fm.getContentFile(fileData, includePath: true);
                    input = JsonConvert.DeserializeObject<InputDocumentosDTO>(data);

                    App.config = new ConectorConfig()
                    {
                        dbInstance = input.instancia,
                        dbDatabase = input.empresaDB,
                        dbUser = input.userSQL,
                        dbPass = input.passSQL,
                        sdkUser = input.userSDK,
                        sdkPass = input.passSDK,
                        sdkEmpresaEnUso = input.rutaEmpresa
                    };
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "Aplicando pagos");
                    App.logs.add($@"error al convertir los datos de pagos {e.StackTrace}");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (data != "")
                {
                    try
                    {
                        ComercialSdkConsumer sdk = new ComercialSdkConsumer(new ComercialSdkConsumerConfigDTO()
                        {
                            userSDK = input.userSDK,
                            passSDK = input.passSDK
                        });
                        sdk.inicializaSdk();
                        sdk.abreEmpresa(input.rutaEmpresa);
                        SQLSRV db = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                        {
                            instancia = input.instancia,
                            db = input.empresaDB,
                            usuario = input.userSQL,
                            contrasena = input.passSQL
                        });
                        db.Conecta();
                        // se acumulan los documentos que se estan procesando para retornar una respuesta
                        List<DocumentoDTO> documentosRespuesta = new List<DocumentoDTO>();
                        foreach (var documento in input.documentos)
                        {
                            var serializedParent = JsonConvert.SerializeObject(documento);
                            DocumentoRespuestaDTO docTemp = JsonConvert.DeserializeObject<DocumentoRespuestaDTO>(serializedParent);
                            //Aqui se valida si todos los UUID existen
                            var totalUUIDs = documento.docRelacionadosNCs.Length;
                            List<string> uuids = new List<string>();

                            //Aqui agrego en una lista a todos los UUID que existe en mi XML
                            foreach (DocRelacionadosNC docRelacionado in documento.docRelacionadosNCs)
                            {
                                uuids.Add($"'{docRelacionado.UUID}'");
                            }

                            try
                            {
                                string uuidsConcatenados = string.Join(",", uuids);
                                var totalDocumentos = db.EjecutarLecturaDeDatos($@" SELECT D.CIDDOCUMENTO, D.CSERIEDOCUMENTO, D.CFOLIO, D.CIDCONCEPTODOCUMENTO, CAST(D.CPENDIENTE AS DECIMAL(18, 2)) AS CPENDIENTE, F.CUUID 
                                FROM admFoliosDigitales F LEFT JOIN admDocumentos D ON F.CIDDOCTO = D.CIDDOCUMENTO 
                                WHERE F.CIDDOCTO <> 0 AND F.CRFC = '{documento.codigoCteProv}' AND D.CPENDIENTE > 0  AND F.CUUID IN ({uuidsConcatenados}) 
                                ORDER BY F.CFECHAEMI ASC");
                                int rowCount = 0; 
                                decimal totalPendiente = 0;
                                var documentosNCs = new List<DocumentoDTONC>();
                                //Crea mi objetos para acceder y proceder a saldar mis documentos

                                var totalDocumentoImporte = documento.importe;
                                decimal abonoMinimo = 1; // Abono mínimo de 1 peso
                                decimal montoRestante = decimal.Parse(totalDocumentoImporte);

                                while (totalDocumentos.Read())
                                {
                                    rowCount++;
                                    if (!totalDocumentos.IsDBNull(4)) // 4 es el índice de CPENDIENTE
                                    {
                                        decimal cpPendiente = totalDocumentos.GetDecimal(4); // Obtener el valor
                                        totalPendiente += cpPendiente; // Sumar CPENDIENTE
                                    }
                                   
                                    var documentonc = new DocumentoDTONC
                                    {
                                        CIDDOCUMENTO = totalDocumentos["CIDDOCUMENTO"] is DBNull ? 0 : Convert.ToInt32(totalDocumentos["CIDDOCUMENTO"]),
                                        CSERIEDOCUMENTO = totalDocumentos["CSERIEDOCUMENTO"] is DBNull ? string.Empty : totalDocumentos["CSERIEDOCUMENTO"].ToString(),
                                        CFOLIO = totalDocumentos["CFOLIO"] is DBNull ? string.Empty : totalDocumentos["CFOLIO"].ToString(),
                                        CIDCONCEPTODOCUMENTO = totalDocumentos["CIDCONCEPTODOCUMENTO"] is DBNull ? string.Empty : Convert.ToString(totalDocumentos["CIDCONCEPTODOCUMENTO"]),
                                        CUUID = totalDocumentos["CUUID"] is DBNull ? string.Empty : totalDocumentos["CUUID"].ToString(),
                                        CPENDIENTE = totalDocumentos["CPENDIENTE"] is DBNull ? 0m : Convert.ToDecimal(totalDocumentos["CPENDIENTE"]) // Asegúrate de que CPENDIENTE sea decimal+
                                        
                                    };
                                    documentosNCs.Add(documentonc);
                                }

                                // Asigna el abono mínimo de 1 peso a cada documento primero
                                foreach (var doc in documentosNCs)
                                {
                                    if (montoRestante < abonoMinimo)
                                    {
                                        throw new Exception("Monto insuficiente para abonar a todos los documentos con el abono mínimo.");
                                    }

                                    // Abona el mínimo a cada documento
                                    doc.CABONO = abonoMinimo;
                                    montoRestante -= abonoMinimo;
                                }

                                // Distribuye el monto restante en orden (ya viene ordenado desde la consulta)
                                foreach (var doc in documentosNCs)
                                {
                                    if (montoRestante <= 0)
                                    {
                                        break; // Detén la distribución si no queda monto
                                    }

                                    // Calcula el monto a abonar sin exceder el saldo pendiente del documento
                                    decimal montoASaldar = Math.Min(montoRestante, doc.CPENDIENTE - doc.CABONO);
                                    doc.CABONO += montoASaldar;
                                    montoRestante -= montoASaldar;
                                }

                                // Mostrar el abono asignado a cada documento
                                foreach (var doc in documentosNCs)
                                {
                                    App.logs.add("********* DATOS A REALIZAR ********");
                                    App.logs.add($"Documento ID: {doc.CIDDOCUMENTO}, Serie: {doc.CSERIEDOCUMENTO}, Folio: {doc.CFOLIO}, " +
                                                      $"Abono: {doc.CABONO}, Saldo Pendiente Final: {doc.CPENDIENTE - doc.CABONO}");
                                    App.logs.add("********* DATOS A REALIZAR ********");
                                }

                                if (rowCount == totalUUIDs)
                                {
                                    App.logs.add($@"------------Se inicia a crear la Notas de Credito / Nota de cargo");
                                    DocumentoRespuestaDTO docRespuesta = sdk.creaDocumento(docTemp);
                                    docRespuesta.FileName = docTemp.FileName;
                                    docRespuesta.docRelacionados = documento.docRelacionados;

                                    if (docRespuesta.error == null)
                                    {
                                        // se consulta el folio insertado y se valida si es que se inserto
                                        if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docId) && docId > 0)
                                        {
                                            docRespuesta.folio = db.ExecuteScalar($"SELECT CFOLIO FROM admDocumentos WHERE CIDDOCUMENTO = {docRespuesta.id}");
                                        }
                                        documentosRespuesta.Add(docRespuesta);
                                    }
                                    if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docIde) && docIde > 0)
                                    {
                                        int affectedRows = db.EjecutaSimpleQuery($"" +
                                            $"UPDATE admDocumentos SET COBSERVACIONES = '{documento.observaciones}', " +
                                            $"CCANTPARCI = '{documento.MetodoPago}',  CLUGAREXPE = '{documento.LugarExp}' , CMETODOPAG= '{documento.FormaPago}'," +
                                            $"CCONDIPAGO = '{documento.CondicionPago}' WHERE CIDDOCUMENTO = {docRespuesta.id}");

                                        int affectedRowsFolios = db.EjecutaSimpleQuery($"UPDATE admFoliosDigitales SET CCODCONCBA = '{documento.UsoCFDI}' , CDESCAUT03 ='{documento.RegimenFiscal}'  WHERE CIDDOCTO = {docRespuesta.id}");
                                        if (affectedRows <= 0)
                                        {
                                            App.logs.add($@"Error: No se pudo actualizar las observaciones del documento {docRespuesta.folio}");
                                        }

                                        foreach (MovimientoDocumentoDTO movimiento in docRespuesta.movimientos)
                                        {
                                            int affectedMovimientos = db.EjecutaSimpleQuery($"UPDATE ADMMOVIMIENTOS SET CTEXTOEXTRA1 = '{movimiento.ctextoExtra1}' WHERE CIDMOVIMIENTO = {movimiento.id}");
                                            if (affectedMovimientos <= 0)
                                            {
                                                App.logs.add($@"Error: No se pudo actualizar el campo CTEXTOEXTRA1 del Movimiento {docRespuesta.folio}");
                                            }
                                        }
                                        int totalRelaciones = documento.docRelacionadosNCs.Length;

                                        foreach (var docNC in documentosNCs)
                                        {
                                            int docRelacionadoNC = sdk.relacionarDocumento(documento.codConcepto, documento.serie, documento.folio, docNC.CUUID, "01");
                                            if (docRelacionadoNC > 0)
                                            {
                                                tLlaveDoc llaveDocumento = new tLlaveDoc();
                                                llaveDocumento.aSerie = docNC.CSERIEDOCUMENTO;
                                                llaveDocumento.aFolio = Convert.ToDouble(docNC.CFOLIO);
                                                llaveDocumento.aCodConcepto = docNC.CIDCONCEPTODOCUMENTO;

                                                tLlaveDoc llavePago = new tLlaveDoc();
                                                llavePago.aSerie = docRespuesta.serie;
                                                llavePago.aFolio = Convert.ToDouble(docRespuesta.folio);
                                                llavePago.aCodConcepto = documento.codConcepto;

                                                int saldadoDocumento = sdk.saldarDocumentoPago(llaveDocumento, llavePago, Convert.ToDouble(docNC.CABONO), int.Parse(documento.numMoneda), documento.fecha);
                                            }
                                            else
                                            {
                                                App.logs.add($@"Error: No se pudo relacionar la factura con folio: {docNC.CSERIEDOCUMENTO} - {docNC.CFOLIO} No se encuentra en sistema");
                                            }
                                        }
                                        //Aqui timbramos el documento pago
                                        if (documento.TimbrarCFDI)
                                        {
                                            Comprobante comprobante = new Comprobante();
                                            comprobante.aSerie = documento.serie;
                                            comprobante.aCodConcepto = documento.codConcepto;
                                            comprobante.aFolio = documento.folio;
                                            comprobante.aPassword = input.PassCSD;
                                            sdk.timbrarDocumento(comprobante);
                                        }
                                    }
                                    else
                                    {
                                        App.logs.add($@"xxxxx ocurrio un error al crear documento xxxxxxx");
                                    }
                                }
                                else
                                {
                                    App.logs.add("El número de documentos no coincide con la cantidad de UUIDs.");
                                }

                            }
                            catch (Exception ex)
                            {
                                App.logs.add($"Se ha producido un error: {ex.Message}");
                            }

                        }
                        
                        db.Desconecta();
                        sdk.finalizaSdk();
                        serializedMesssage = App.responseCLI.serializeMessage(true, "NC procesados", documentosRespuesta);
                        App.logs.add($@"Notas procesadas correctamente");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);
                    }
                    catch (Exception e)
                    {
                        App.logs.add($"Error al leer los documentos");
                        App.logs.add(e.StackTrace);
                    }
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de pagos no válidos", "App", "aplicando pagos");
                    App.logs.add($@"datos de notas no validos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "aplicando pagos");
                Console.WriteLine(serializedMesssage);
                App.logs.add($@"Archivo de datos no encontrado {fileData}");
                Environment.Exit(0);
            }
        }

        public static void saldarDocumentos(string fileData)
        {
            FileManager fm = new FileManager();
            App.logs.add($@"leyendo archivo -Documento Pago {fileData}");
            if (fm.checkIfExists(fileData))
            {
                string data = "";
                string serializedMesssage;
                InputDocumentosDTO input = new InputDocumentosDTO();
                try
                {
                    data = fm.getContentFile(fileData, includePath: true);
                    input = JsonConvert.DeserializeObject<InputDocumentosDTO>(data);

                    App.config = new ConectorConfig()
                    {
                        dbInstance = input.instancia,
                        dbDatabase = input.empresaDB,
                        dbUser = input.userSQL,
                        dbPass = input.passSQL,
                        sdkUser = input.userSDK,
                        sdkPass = input.passSDK,
                        sdkEmpresaEnUso = input.rutaEmpresa
                    };
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "Aplicando pagos");
                    App.logs.add($@"error al convertir los datos de pagos {e.StackTrace}");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (data != "")
                {
                    try
                    {
                        ComercialSdkConsumer sdk = new ComercialSdkConsumer(new ComercialSdkConsumerConfigDTO()
                        {
                            userSDK = input.userSDK,
                            passSDK = input.passSDK
                        });
                        sdk.inicializaSdk();
                        sdk.abreEmpresa(input.rutaEmpresa);
                        SQLSRV db = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                        {
                            instancia = input.instancia,
                            db = input.empresaDB,
                            usuario = input.userSQL,
                            contrasena = input.passSQL
                        });
                        db.Conecta();
                        // se acumulan los documentos que se estan procesando para retornar una respuesta
                        List<DocumentoDTO> documentosRespuesta = new List<DocumentoDTO>();
                        foreach (var documento in input.documentos)
                        {
                            var serializedParent = JsonConvert.SerializeObject(documento);
                            DocumentoRespuestaDTO docTemp = JsonConvert.DeserializeObject<DocumentoRespuestaDTO>(serializedParent);
                            DocumentoRespuestaDTO docRespuesta = sdk.creaDocumento(docTemp);
                            docRespuesta.FileName = docTemp.FileName;
                            docRespuesta.docRelacionados = documento.docRelacionados;

                            if (docRespuesta.error == null)
                            {
                                // se consulta el folio insertado y se valida si es que se inserto
                                if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docId) && docId > 0)
                                {
                                    docRespuesta.folio = db.ExecuteScalar($"SELECT CFOLIO FROM admDocumentos WHERE CIDDOCUMENTO = {docRespuesta.id}");
                                }

                                documentosRespuesta.Add(docRespuesta);
                            }
                            if (docRespuesta.id != null || int.TryParse(docRespuesta.id, out int docIde) && docIde > 0)
                            {
                                int affectedRows = db.EjecutaSimpleQuery($"" +
                                    $"UPDATE admDocumentos SET COBSERVACIONES = '{documento.observaciones}', " +
                                    $"CCANTPARCI = '{documento.MetodoPago}',  CLUGAREXPE = '{documento.LugarExp}' , CMETODOPAG= '{documento.FormaPago}'," +
                                    $"CCONDIPAGO = '{documento.CondicionPago}' WHERE CIDDOCUMENTO = {docRespuesta.id}");

                                int affectedRowsFolios = db.EjecutaSimpleQuery($"UPDATE admFoliosDigitales SET CCODCONCBA = '{documento.UsoCFDI}' , CDESCAUT03 ='{documento.RegimenFiscal}'  WHERE CIDDOCTO = {docRespuesta.id}");
                                if (affectedRows <= 0)
                                {
                                    App.logs.add($@"Error: No se pudo actualizar las observaciones del documento {docRespuesta.folio}");
                                }
                                foreach (MovimientoDocumentoDTO movimiento in docRespuesta.movimientos)
                                {
                                    int affectedMovimientos = db.EjecutaSimpleQuery($"UPDATE ADMMOVIMIENTOS SET CTEXTOEXTRA1 = '{movimiento.ctextoExtra1}' WHERE CIDMOVIMIENTO = {movimiento.id}");
                                    if (affectedMovimientos <= 0)
                                    {
                                        App.logs.add($@"Error: No se pudo actualizar el campo CTEXTOEXTRA1 del Movimiento {docRespuesta.folio}");
                                    }
                                }

                                int totalRelaciones = documento.docRelacionados.Length;
                                int BanderaRelacionesSuccess = 0;
                                foreach (DocumentoRelacionadoDTO docRelacionado in documento.docRelacionados)
                                {
                                    
                                    docRelacionado.ConceptoDocumento = db.ExecuteScalar($"SELECT CIDCONCEPTODOCUMENTO FROM admDocumentos WHERE  CSERIEDOCUMENTO = '{docRelacionado.Serie}' and CFOLIO  = '{docRelacionado.Folio}' ");

                                    if (docRelacionado.ConceptoDocumento != null) {

                                        tLlaveDoc llaveDocumento = new tLlaveDoc();
                                        llaveDocumento.aSerie = docRelacionado.Serie;
                                        llaveDocumento.aFolio = Convert.ToDouble(docRelacionado.Folio);
                                        llaveDocumento.aCodConcepto = docRelacionado.ConceptoDocumento;

                                        tLlaveDoc llavePago = new tLlaveDoc();
                                        llavePago.aSerie = docRespuesta.serie;
                                        llavePago.aFolio = Convert.ToDouble(docRespuesta.folio);
                                        llavePago.aCodConcepto = documento.codConcepto;

                                        int saldadoDocumento = sdk.saldarDocumentoPago(llaveDocumento, llavePago, docRelacionado.ImpPagado, docRelacionado.MonedaDR == "USD" ? 2 : 1 , documento.fecha);
                                        if(saldadoDocumento > 0)
                                        {
                                            BanderaRelacionesSuccess++;
                                        }
                                    }
                                    else
                                    {
                                        App.logs.add($@"Error: la factura con folio: {docRelacionado.Serie} - {docRelacionado.Folio} No se encuentra en sistema");
                                    }
                                }
                                if(totalRelaciones != BanderaRelacionesSuccess)
                                {
                                    App.logs.add($@"El sistema por seguridad no timbro el pago, tendra que realizar las relaciones manualmente");
                                }
                                else
                                {
                                    //Aqui timbramos el documento pago
                                    if (documento.TimbrarCFDI)
                                    {
                                        Comprobante comprobante = new Comprobante();
                                        comprobante.aSerie = documento.serie;
                                        comprobante.aCodConcepto = documento.codConcepto;
                                        comprobante.aFolio = documento.folio;
                                        comprobante.aPassword = input.PassCSD;
                                        sdk.timbrarDocumento(comprobante);
                                    }
                                    
                                }
                            }
                        }
                        db.Desconecta();
                        sdk.finalizaSdk();
                        serializedMesssage = App.responseCLI.serializeMessage(true, "Pagos procesados", documentosRespuesta);
                        App.logs.add($@"Pagos procesados");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);

                    }
                    catch (Exception e)
                    {
                        App.logs.add($"Error al procesar los pagos");
                        App.logs.add(e.StackTrace);
                    }
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de pagos no válidos", "App", "aplicando pagos");
                    App.logs.add($@"datos de pagos no validos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "aplicando pagos");
                Console.WriteLine(serializedMesssage);
                App.logs.add($@"Archivo de datos no encontrado {fileData}");
                Environment.Exit(0);
            }
        }

        /**
         * <summary>
         *  Se consultan las existencias un arreglo productos en el sistema comercial
         * </summary>
         */
        public static void consultarExistenciasProductos(string fileData)
        {
            FileManager fm = new FileManager();
            App.logs.add($@"leyendo archivo {fileData}");
            if (fm.checkIfExists(fileData))
            {
                string data = "";
                string serializedMesssage;
                InputConsultaExistenciaProductoDTO input = new InputConsultaExistenciaProductoDTO();
                try
                {
                    data = fm.getContentFile(fileData, includePath: true);
                    input = JsonConvert.DeserializeObject<InputConsultaExistenciaProductoDTO>(data);

                    App.config = new ConectorConfig()
                    {
                        dbInstance = input.instancia,
                        dbDatabase = input.empresaDB,
                        dbUser = input.userSQL,
                        dbPass = input.passSQL,
                        sdkUser = input.userSDK,
                        sdkPass = input.passSDK,
                        sdkEmpresaEnUso = input.rutaEmpresa
                    };
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Error al convertir los datos json", "App", "consultarExistenciasProductos");
                    App.logs.add($@"error al convertir los datos de la consulta de existencias {e.StackTrace}");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (data != "")
                {
                    try
                    {
                        ComercialSdkConsumer sdk = new ComercialSdkConsumer(new ComercialSdkConsumerConfigDTO()
                        {
                            userSDK = input.userSDK,
                            passSDK = input.passSDK
                        });
                        sdk.inicializaSdk();
                        sdk.abreEmpresa(input.rutaEmpresa);
                        // se inicializa un objeto global para  sql server
                        //SQLSRV db = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                        //{
                        //    instancia = input.instancia,
                        //    db = input.empresaDB,
                        //    usuario = input.userSQL,
                        //    contrasena = input.passSQL
                        //});
                        SqlServer.createManager(new DTOs.ConfiguracionSqlServerDTO()
                        {
                            instancia = input.instancia,
                            db = input.empresaDB,
                            usuario = input.userSQL,
                            contrasena = input.passSQL
                        });

                        // se inicializa el proceso para la consulta de existencias de los productos
                        List<ConsultaExistenciasProductoRespuestaDto> productosExistenciasRespuesta = new List<ConsultaExistenciasProductoRespuestaDto>();

                        foreach(ConsultaExistenciasProductoDto prodParams in input.productos)
                        {
                            // se consultan las existencias por producto
                            ConsultaExistenciasProductoRespuestaDto respuesta = sdk.getExistenciasProducto(prodParams);
                            productosExistenciasRespuesta.Add(respuesta);
                        }

                        SqlServer.Manager.Desconecta();
                        sdk.finalizaSdk();
                        serializedMesssage = App.responseCLI.serializeMessage(true, "Consultas de existencias procesadas", productosExistenciasRespuesta.ToArray());
                        App.logs.add($@"Consultas de existencias procesadas");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);

                    }
                    catch (Exception e)
                    {
                        App.logs.add($"Error al procesar la consulta de existencias");
                        App.logs.add(e.StackTrace);
                    }
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de consulta de existencias no válidos", "App", "consultarExistenciasProductos");
                    App.logs.add($@"datos de documentos no validos");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            else
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, "Archivo de datos no encontrado", "App", "consultarExistenciasProductos");
                Console.WriteLine(serializedMesssage);
                App.logs.add($@"Archivo de datos no encontrado {fileData}");
                Environment.Exit(0);
            }
        }
        #endregion

        #region acciones_configuracion_conector

        public static void guardaConfiguracionSqlServer(string archivoDatos)
        {
            FileManager fm = new FileManager(App.config.appDir);
            if (fm.checkIfExists(archivoDatos))
            {
                string dataConfig = "";
                DTOs.ConfiguracionSqlServerDTO config = new DTOs.ConfiguracionSqlServerDTO();
                string serializedMesssage;
                try
                {
                    dataConfig = fm.getContentFile(archivoDatos, includePath: true);
                    config = JsonConvert.DeserializeObject<DTOs.ConfiguracionSqlServerDTO>(dataConfig);
                }
                catch (Exception e)
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "Guardando configuracion de Sql Server");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }

                if (dataConfig != "")
                {
                    // guardar la configuracion de sql server
                    App.regManager.guardaLlave("dbInstance", config.instancia);
                    App.regManager.guardaLlave("dbDatabase", config.db);
                    App.regManager.guardaLlave("dbUser", config.usuario);
                    App.regManager.guardaLlave("dbPass", config.contrasena);
                    serializedMesssage = App.responseCLI.serializeMessage(true, "Configuracion de Sql Server guardada");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
                else
                {
                    serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de configuracion no válidos", "App", "Guardando configuracion de Sql Server");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
        }

        public static void consultarConfigSqlServer()
        {
            string serializedMesssage;
            try
            {
                DTOs.ConfiguracionSqlServerDTO config = new DTOs.ConfiguracionSqlServerDTO()
                {
                    instancia = App.regManager.ObtenerValorDeClave("dbInstance"),
                    usuario = App.regManager.ObtenerValorDeClave("dbUser"),
                    contrasena = App.regManager.ObtenerValorDeClave("dbPass"),
                    db = App.regManager.ObtenerValorDeClave("dbDatabase"),
                };
                serializedMesssage = App.responseCLI.serializeMessage(true, "Configuracion de Sql Server consultada", config);
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "consultando configuracion de sql server");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }

        }

        public static void consultarConfigContpaq()
        {
            string serializedMesssage;
            try
            {
                DTOs.ConfiguracionContpaqDTO config = new DTOs.ConfiguracionContpaqDTO()
                {
                    usuario = App.regManager.ObtenerValorDeClave("sdkUser"),
                    contrasena = App.regManager.ObtenerValorDeClave("sdkPass"),
                    empresaEnUso = App.regManager.ObtenerValorDeClave("sdkEmpresaEnUso")
                };

                serializedMesssage = App.responseCLI.serializeMessage(true, "Configuracion de CONTPAQi Comercial consultada", config);
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "consultando configuracion de CONTPAQi Comercial");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }

        }

        public static void guardaConfiguracionContpaq(string archivoDatos)
        {
            try
            {

                FileManager fm = new FileManager(App.config.appDir);
                if (fm.checkIfExists(archivoDatos))
                {
                    string dataConfig = "";
                    DTOs.ConfiguracionContpaqDTO config = new DTOs.ConfiguracionContpaqDTO();
                    string serializedMesssage;
                    try
                    {
                        dataConfig = fm.getContentFile(archivoDatos, includePath: true);
                        config = JsonConvert.DeserializeObject<DTOs.ConfiguracionContpaqDTO>(dataConfig);

                    }
                    catch (Exception e)
                    {
                        serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de configuracion no válidos", "App", "Guardando configuracion de Contpaq Comercial");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);
                    }

                    if (dataConfig != "")
                    {
                        App.regManager.guardaLlave("sdkUser", config.usuario);
                        App.regManager.guardaLlave("sdkPass", config.contrasena);
                        App.regManager.guardaLlave("sdkEmpresaEnUso", config.empresaEnUso);
                        // se guardan los datos extra de la configuracion
                        serializedMesssage = App.responseCLI.serializeMessage(true, "Configuracion de Contpaqi Comercial guardada");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);
                    }
                    else
                    {
                        serializedMesssage = App.responseCLI.serializeMessage(false, "Datos de configuracion no validos", "App", "Guardando configuracion de Contpaq Comercial");
                        Console.WriteLine(serializedMesssage);
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
        }

        public static void consultaInstanciasSqlServer()
        {
            // This procedure is provider-agnostic, and can list
            // instances of any provider's servers. Of course, 
            // not all providers can create a data source enumerator,
            // so it's best to check the CanCreateDataSourceEnumerator 
            // property before attempting to list the data sources.


            string serializedMesssage;
            try
            {
                List<DTOs.InstanciaSqlServerDTO> instancias = new List<DTOs.InstanciaSqlServerDTO>();
                using (DataTable sqlSources = SqlDataSourceEnumerator.Instance.GetDataSources())
                {
                    foreach (DataRow source in sqlSources.Rows)
                    {
                        string instanceName = source["InstanceName"].ToString();

                        if (!string.IsNullOrEmpty(instanceName))
                        {
                            DTOs.InstanciaSqlServerDTO instancia = new DTOs.InstanciaSqlServerDTO()
                            {
                                ServerName = source["ServerName"].ToString(),
                                InstanceName = source["InstanceName"].ToString(),
                                Version = source["Version"].ToString()
                            };
                            instancias.Add(instancia);
                        }
                    }
                }
                serializedMesssage = App.responseCLI.serializeMessage(true, "Instancias Consultadas", instancias);
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
            catch (Exception e)
            {
                serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "Consultando instancias de sql server");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }


        #endregion

        #region consultas_sql_server
        /*
        * se consultan las empresas de comercial y se envian en el campo de datos
        * de la respuesta del conector con tipo EmpresaDTO
        */
        public static void consultaEmpresas()
        {
            try
            {
                if (App.config.dbInstance != "")
                {

                    SQLSRV sqlManager = new SQLSRV(new DTOs.ConfiguracionSqlServerDTO()
                    {
                        instancia = App.config.dbInstance,
                        db = "CompacWAdmin",
                        usuario = App.config.dbUser,
                        contrasena = App.config.dbPass
                    });
                    List<DTOs.EmpresaDTO> empresas = new List<DTOs.EmpresaDTO>();
                    sqlManager.Conecta();
                    SqlDataReader reader = sqlManager.EjecutarLecturaDeDatos("select CNOMBREEMPRESA as nombre, CRUTADATOS as rutaDatos from empresas where CNOMBREEMPRESA <> '(Predeterminada)'");
                    if (reader.HasRows)
                        while (reader.Read())
                        {
                            empresas.Add(new DTOs.EmpresaDTO()
                            {
                                nombre = reader["nombre"].ToString(),
                                rutaDatos = reader["rutaDatos"].ToString()
                            }); ;
                        }
                    sqlManager.Desconecta();

                    string serializedMesssage = App.responseCLI.serializeMessage(true, "Empresas consultadas", empresas);
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
                else
                {
                    string serializedMesssage = App.responseCLI.serializeMessage(false, "configuracion requerida", "App", "consultando empresas");
                    Console.WriteLine(serializedMesssage);
                    Environment.Exit(0);
                }
            }
            catch (Exception e)
            {
                string serializedMesssage = App.responseCLI.serializeMessage(false, e.Message, "App", "consultando empresas");
                Console.WriteLine(serializedMesssage);
                Environment.Exit(0);
            }
        }
        #endregion
    }
}
