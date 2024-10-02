using ConectorComercialCLI.DTOs;
using ConectorComercialCLI.DTOs.Almacenes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ConectorComercialCLI.Sevices
{
    public class AlmacenesService
    {
        public AlmacenesService() { }

        public ConsultaAlmacenesRespuestaDto getAlmacenes(ConsultaAlmacenesDto data)
        {
            try
            {
                List<AlmacenDto> almacenes = new List<AlmacenDto>();
                List<string> codAlmacenes = new List<string>();
                string query = "";
                if (data.codAlmacen != "" && data.codAlmacen != null)
                {
                    codAlmacenes.Add(data.codAlmacen);
                }
                else
                {
                    // se consultan los almacenes del producto si es que se recibe un codigo de producto
                    query = $@"
                        SELECT almacenes.CCODIGOALMACEN FROM admExistenciaCosto existencias
                        INNER JOIN admProductos productos ON existencias.CIDPRODUCTO = productos.CIDPRODUCTO
                        INNER JOIN admAlmacenes almacenes ON almacenes.CIDALMACEN = existencias.CIDALMACEN
                        WHERE CCODIGOPRODUCTO = '{data.codProducto}'
                        GROUP BY almacenes.CCODIGOALMACEN
                    ";
                    // se convierte el reader de almacenes en lista
                    using (SqlDataReader reader = SqlServer.Manager.EjecutarLecturaDeDatos(query))
                    {
                        while (reader.Read())
                        {
                            codAlmacenes.Add(reader.GetString(0).Trim());
                        }
                    }
                }

                query = $"SELECT * FROM admAlmacenes WHERE CCODIGOALMACEN IN ('{String.Join("','", codAlmacenes)}')";
                // en caso de no recibir el codigo de producto se consulta el almacen indicado
                using (SqlDataReader reader = SqlServer.Manager.EjecutarLecturaDeDatos(query))
                {
                    while (reader.Read())
                    {
                        AlmacenDto almacen = new AlmacenDto();
                        almacen.CIDALMACEN = reader.GetInt32(reader.GetOrdinal("CIDALMACEN"));
                        almacen.CCODIGOALMACEN = reader.GetString(reader.GetOrdinal("CCODIGOALMACEN")).Trim();
                        almacen.CNOMBREALMACEN = reader.GetString(reader.GetOrdinal("CNOMBREALMACEN")).Trim();
                        almacen.CFECHAALTAALMACEN = reader.GetDateTime(reader.GetOrdinal("CFECHAALTAALMACEN"));
                        almacen.CIDVALORCLASIFICACION1 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION1"));
                        almacen.CIDVALORCLASIFICACION2 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION2"));
                        almacen.CIDVALORCLASIFICACION3 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION3"));
                        almacen.CIDVALORCLASIFICACION4 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION4"));
                        almacen.CIDVALORCLASIFICACION5 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION5"));
                        almacen.CIDVALORCLASIFICACION6 = reader.GetInt32(reader.GetOrdinal("CIDVALORCLASIFICACION6"));
                        almacen.CSEGCONTALMACEN = reader.GetString(reader.GetOrdinal("CSEGCONTALMACEN")).Trim();
                        almacen.CTEXTOEXTRA1 = reader.GetString(reader.GetOrdinal("CTEXTOEXTRA1")).Trim();
                        almacen.CTEXTOEXTRA2 = reader.GetString(reader.GetOrdinal("CTEXTOEXTRA2")).Trim();
                        almacen.CTEXTOEXTRA3 = reader.GetString(reader.GetOrdinal("CTEXTOEXTRA3")).Trim();
                        almacen.CIMPORTEEXTRA1 = reader.GetDouble(reader.GetOrdinal("CIMPORTEEXTRA1"));
                        almacen.CIMPORTEEXTRA2 = reader.GetDouble(reader.GetOrdinal("CIMPORTEEXTRA2"));
                        almacen.CIMPORTEEXTRA3 = reader.GetDouble(reader.GetOrdinal("CIMPORTEEXTRA3"));
                        almacen.CIMPORTEEXTRA4 = reader.GetDouble(reader.GetOrdinal("CIMPORTEEXTRA4"));
                        almacen.CBANDOMICILIO = reader.GetInt32(reader.GetOrdinal("CBANDOMICILIO"));
                        almacen.CSCALMAC2 = reader.GetString(reader.GetOrdinal("CSCALMAC2")).Trim();
                        almacen.CSCALMAC3 = reader.GetString(reader.GetOrdinal("CSCALMAC3")).Trim();
                        almacenes.Add(almacen);
                    }
                }

                return new ConsultaAlmacenesRespuestaDto()
                {
                    almacenes = almacenes,
                    error = null
                };
            }
            catch (Exception ex)
            {
                return new ConsultaAlmacenesRespuestaDto()
                {
                    almacenes = new List<AlmacenDto>(),
                    error = new ErrorDTO()
                    {
                        code = 1,
                        message = ex.Message
                    }
                };
            }
        }

    }
}
