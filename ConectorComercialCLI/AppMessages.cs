using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ConectorComercialCLI
{
    /*
     * manejador de respuestas del conector
    */
    class AppMessages
    {
        public bool success { get; set; }
        public string message { get; set; }
        public string type { get; set; }
        public string origen { get; set; }
        public Object data { get; set; }

        /*
         * El metodo serializeMessage se sobrecarga recibiendo diferentes parametros,
         * cada uno con su proposito
         

         * [boolean] success: especifica el estado de la respuesta
         *  - true: indica que la accion solicitada se ejecuto con exito
         *  - false: indica que hay algun error con la accion solicitada o que se lanzo alguna Excepcion
         
         * [string] message: mensaje de retroalimentacion del conector (generico)
         
         * [string] type: si hay algun error en el conector este parametro
         *                ayuda a indicar si el error se ha generado por el conector,
         *                el sdk, algun manejador u otra herramienta utilizada por el conector
         *                (el proposito principal de este parametro es para retroalimentacion de errores generados por el conector)
         
         * [string] origen: este parametro ayuda a especificar un dato extra en la respuesta del conector
         *                  por ejemplo cuando se inserta un recurso en contpaq se 
         *                  puede especificar el id generado para utilizarlo en otra accion
         *                  
         *                  o como segundo proposito, especificar el metodo, funcion o accion que lanzo algun error
         *                  con fin de ayudar en el debug
         
         * [object] data: objeto generico que ayuda a retornar datos en las respuestas del conector
         *                como por ejemplo cuando se consulta un recurso, los datos del recurso se pueden
         *                especificar en este parametro
         
        */


        public string serializeMessage(bool success, string message)
        {
            this.data = null;
            this.origen = null;
            this.type = null;

            this.success = success;
            this.message = message;
            return JsonConvert.SerializeObject(this);
        }

        public string serializeMessage(bool success, string message, Object data)
        {
            this.origen = null;
            this.type = null;

            this.data = data;
            this.success = success;
            this.message = message;
            return JsonConvert.SerializeObject(this);
        }

        public string serializeMessage(bool success, string message, string type)
        {
            this.data = null;
            this.origen = null;

            this.success = success;
            this.message = message;
            this.type = type;

            return JsonConvert.SerializeObject(this);
        }

        public string serializeMessage(bool success, string message, string type, string origen)
        {
            this.data = null;

            this.success = success;
            this.message = message;
            this.type = type;
            this.origen = origen;

            return JsonConvert.SerializeObject(this);
        }

        public string serializeMessage(bool success, string message, Object data, string origen)
        {
            this.type = null;

            this.success = success;
            this.message = message;
            this.origen = origen;
            this.data = data;

            return JsonConvert.SerializeObject(this);
        }
    }
}
