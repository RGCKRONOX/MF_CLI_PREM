using System;
using System.IO;

namespace ConectorComercialCLI
{
    public class AppLogs
    {
        private string logDir { get; set; }
        private string logsFilePath { get; set; }

        public AppLogs(string logDir)
        {
            this.logDir = logDir;
            this.logsFilePath = $@"{logDir}\logs.txt";
            this.creaArchivo();
        }

        private void creaArchivo()
        {
            try
            {
                if (!Directory.Exists(this.logDir))
                {
                    Directory.CreateDirectory(this.logDir);
                }

                if (!System.IO.File.Exists(this.logsFilePath))
                {
                    FileStream fs = System.IO.File.Create(this.logsFilePath);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void agregaLinea(string pLinea)
        {
            try
            {
                using (StreamWriter sw = System.IO.File.AppendText(this.logsFilePath))
                {
                    sw.WriteLine(pLinea);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void add(string pMensaje)
        {
            this.agregaLinea($"{DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss") } - " + pMensaje);
        }

    }
}
