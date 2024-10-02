using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

/// <summary>
/// Summary description for Class1
/// </summary>
/// 
namespace ConectorComercialCLI
{
    /*
     * Manejador de consultas de SQL Server.
     * todas las acciones que se necesiten realizar con sql server
     * hay que definirlas aqui como metodo
    */
    public class SQLSRV
    {
        public SqlConnection gSqlConnection = new SqlConnection();

        public SQLSRV(DTOs.ConfiguracionSqlServerDTO configSqlServer)
        {
            //try
            //{
                string gCadenaConexion =
                            $@"Data Source={configSqlServer.instancia};
                                Initial Catalog={configSqlServer.db};
                                Integrated Security=False;
                                MultipleActiveResultSets=True;
                                User Id={configSqlServer.usuario};
                                Password={configSqlServer.contrasena};TrustServerCertificate=true;";

                this.gSqlConnection.ConnectionString = gCadenaConexion.Trim();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Error: {ex.Message}");
            //}
        }

        public int Conecta()
        {
            //try
            //{
                if (this.gSqlConnection.State == ConnectionState.Closed)
                {
                    this.gSqlConnection.Open();
                }

                return 1;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    App.logs.add("Error Conecta: " + gSqlConnection.DataSource + ex.StackTrace);
            //    return 0;
            //}
        }

        public void Desconecta()
        {
            //try
            //{
                this.gSqlConnection.Close();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
        }

        public SqlDataReader EjecutarLecturaDeDatos(string p_sentencia)
        {
            //try
            //{
                SqlCommand comando = new SqlCommand(p_sentencia, this.gSqlConnection);
                return comando.ExecuteReader();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    App.logs.add("Error - SQL Lectura de datos : " + p_sentencia + " - " + ex.Message);
            //    return null;
            //}
        }

        public string ExecuteScalar(string p_sentencia)
        {
            try
            {
                using (SqlCommand comando = new SqlCommand(p_sentencia, this.gSqlConnection))
                {
                    var resultado = comando.ExecuteScalar();
                    return resultado?.ToString(); // Usar el operador null-conditional (?.) para evitar la excepción
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                App.logs.add("Error - SQL Lectura de datos : " + p_sentencia + " - " + ex.Message);
                return null;
            }
        }

        public int EjecutaSimpleQuery(string query)
        {
            //try
            //{
            SqlCommand comando = new SqlCommand(query, this.gSqlConnection);
            return comando.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    App.logs.add("Error - SQL Lectura de datos : " + p_sentencia + " - " + ex.Message);
            //    return null;
            //}
        }
    }
}
