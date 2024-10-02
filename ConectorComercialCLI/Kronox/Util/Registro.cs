using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kronox.Util
{

    public class Registro
    {
        private string gNombreClave;
        private RegistryKey regKey;
        private Dictionary<string, string> listaLlaves;

        public Registro(string regPath, string keyType, Dictionary<string, string> listaLlaves)
        {
            this.listaLlaves = listaLlaves;
            this.ConfigRegEdit(regPath, keyType);
            if (VerificaSiExisteClave("appDir") == false && VerificaSiExisteClave("logDir") == false)
            {
                this.GuardaListaLlaves(listaLlaves);
            }
        }

        private void ConfigRegEdit(string pNombreClave, string p_tipo_de_registro)
        {
            var key = Registry.CurrentUser.OpenSubKey(pNombreClave, true);
            if (key == null)
            {
               this.regKey = Registry.CurrentUser.CreateSubKey(pNombreClave);
            }
            else
            {
                this.regKey = key;
            }

            this.gNombreClave = pNombreClave;
            //if (p_tipo_de_registro == "currentUser")
            //    this.regKey = Registry.CurrentUser.OpenSubKey(this.gNombreClave);
            //else if (p_tipo_de_registro == "localMachine")
            //    this.regKey = Registry.LocalMachine.OpenSubKey(this.gNombreClave);
        }

        public void verificaLlavesRegistro()
        {

        }

        public void GuardaListaLlaves(Dictionary<string, string> pListaClaves)
        {
            try
            {
                foreach (var clave in pListaClaves)
                    this.regKey.SetValue(clave.Key, clave.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void guardaLlave(string llave, string valor)
        {
            try
            {
                this.regKey.SetValue(llave, valor);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string ObtenerValorDeClave(string pClave)
        {
            try
            {
                return this.regKey.GetValue(pClave).ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return String.Empty;
            }
        }

        public bool VerificaSiExisteClave(string pClave)
        {
            try
            {
                if (this.regKey.GetValue(pClave) == null)
                    return false;
                else
                    return true;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }

}
