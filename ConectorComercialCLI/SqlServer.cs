using ConectorComercialCLI.DTOs;
using System.Net;
using System.Threading;

namespace ConectorComercialCLI
{
    public static class SqlServer
    {

        public static SQLSRV Manager { get; set; }

        public static SQLSRV createManager(ConfiguracionSqlServerDTO config)
        {
            SQLSRV manager = new SQLSRV(config);
            if (SqlServer.Manager == null)
            {
                SqlServer.Manager = manager;
                SqlServer.Manager.Conecta();
                return SqlServer.Manager;
            }
            else
            {
                SqlServer.Manager.Desconecta();
                SqlServer.Manager = manager;
                SqlServer.Manager.Conecta();
                return SqlServer.Manager;
            }
        }
    }
}
