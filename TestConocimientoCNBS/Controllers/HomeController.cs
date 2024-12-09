using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using TestConocimientoCNBS.Models;
using TestConocimientoCNBS.Services;
using DotNetEnv;

namespace TestConocimientoCNBS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            Env.Load();
        }
        // METODO PARA OBTENER LOS DATOS DE LAS DECLARACIONES Y UTILIZARLAS EN EL TEMPLATE Privacy.html DONDE SE TIENE LA TABLA DE DECLARACIONES.
        public async Task<IActionResult> Privacy(string query, int numeroPagina = 1)
        {
            var enlaceConexion = Env.GetString("DB_CONNECTION_STRING");
            var declaraciones = new List<Declaraciones>();
            int itemsPerPage = 10;

            using (var conexion = new SqlConnection(enlaceConexion))
            {
                await conexion.OpenAsync();

                int offset = (numeroPagina - 1) * itemsPerPage;

                string sql = string.IsNullOrEmpty(query)
                    ? "SELECT * FROM Declaraciones ORDER BY Dddtoficia OFFSET @offset ROWS FETCH NEXT @itemsPerPage ROWS ONLY"
                    : "SELECT * FROM Declaraciones WHERE Nddtimmioe LIKE @query ORDER BY Dddtoficia OFFSET @offset ROWS FETCH NEXT @itemsPerPage ROWS ONLY";

                using (var comando = new SqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@offset", offset);
                    comando.Parameters.AddWithValue("@itemsPerPage", itemsPerPage);

                    if (!string.IsNullOrEmpty(query))
                    {
                        comando.Parameters.AddWithValue("@query", $"%{query}%");
                    }

                    using (var reader = await comando.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            declaraciones.Add(new Declaraciones
                            {
                                Iddtextr = reader["Iddtextr"].ToString(),
                                Cddtver = reader.GetInt32(reader.GetOrdinal("Cddtver")),
                                Iddtext = reader["Iddtext"].ToString(),
                                Iddt = reader["Iddt"].ToString(),
                                Iext = reader["Iext"].ToString(),
                                Cddteta = reader["Cddteta"].ToString(),
                                Dddtoficia = reader.GetDateTime(reader.GetOrdinal("Dddtoficia")),
                                Dddtrectifa = reader.IsDBNull(reader.GetOrdinal("Dddtrectifa"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtrectifa")),
                                Cddtcirvis = reader["Cddtcirvis"].ToString(),
                                Qddttaxchg = reader.GetDecimal(reader.GetOrdinal("Qddttaxchg")),
                                Ista = reader["Ista"].ToString(),
                                Cddtbur = reader["Cddtbur"].ToString(),
                                Cddtburdst = reader["Cddtburdst"].ToString(),
                                Cddtdep = reader["Cddtdep"].ToString(),
                                Cddtentrep = reader["Cddtentrep"].ToString(),
                                Cddtage = reader["Cddtage"].ToString(),
                                Cddtagr = reader["Cddtagr"].ToString(),
                                Lddtagr = reader["Lddtagr"].ToString(),
                                Nddtimmioe = reader["Nddtimmioe"].ToString(),
                                Lddtnomioe = reader["Lddtnomioe"].ToString(),
                                Cddtpayori = reader["Cddtpayori"].ToString(),
                                Cddtpaidst = reader["Cddtpaidst"].ToString(),
                                Lddtnomfod = reader["Lddtnomfod"].ToString(),
                                Cddtincote = reader["Cddtincote"].ToString(),
                                Cddtdevfob = reader["Cddtdevfob"].ToString(),
                                Cddtdevfle = reader["Cddtdevfle"].ToString(),
                                Cddtdevass = reader["Cddtdevass"].ToString(),
                                Cddttransp = reader["Cddttransp"].ToString(),
                                Cddtmdetrn = reader["Cddtmdetrn"].ToString(),
                                Cddtpaytrn = reader["Cddtpaytrn"].ToString(),
                                Nddtart = reader.GetInt32(reader.GetOrdinal("Nddtart")),
                                Nddtdelai = reader.IsDBNull(reader.GetOrdinal("Nddtdelai"))
                                ? (int?)null
                                : reader.GetInt32(reader.GetOrdinal("Nddtdelai")),
                                Dddtbae = reader.IsDBNull(reader.GetOrdinal("Dddtbae"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtbae")),
                                Dddtsalida = reader.IsDBNull(reader.GetOrdinal("Dddtsalida"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtsalida")),
                                Dddtcancel = reader.IsDBNull(reader.GetOrdinal("Dddtcancel"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtcancel")),
                                Dddtechean = reader.IsDBNull(reader.GetOrdinal("Dddtechean"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtechean")),
                                Cddtobs = reader["Cddtobs"].ToString(),
                                NroTransaccion = reader["NroTransaccion"].ToString(),
                                FechaAConsultar = reader["FechaAConsultar"].ToString(),
                            });
                        }
                    }
                }

                string countSql = string.IsNullOrEmpty(query)
                    ? "SELECT COUNT(*) FROM Declaraciones"
                    : "SELECT COUNT(*) FROM Declaraciones WHERE Nddtimmioe LIKE @query";

                using (var countCommand = new SqlCommand(countSql, conexion))
                {
                    if (!string.IsNullOrEmpty(query))
                    {
                        countCommand.Parameters.AddWithValue("@query", $"%{query}%");
                    }

                    int totalItems = (int)await countCommand.ExecuteScalarAsync();
                    ViewData["totalPaginas"] = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                }
            }

            ViewData["Query"] = query;
            ViewData["paginaActual"] = numeroPagina;

            return View(declaraciones);
        }

        // METODO PARA OBTENER EL DETALLE DE CIERTA DECLARACION CON LIQUIDACION, ARTICULOS, Y LIQUIDACION DE ARTICULOS
        public ActionResult Detalle(string nddtimmioe, string iddt,string NroTransaccion)
        {
            var enlaceConexion = Env.GetString("DB_CONNECTION_STRING");
            var declaraciones = new Declaraciones();
           
            using (var conexion = new SqlConnection(enlaceConexion))
            {
                conexion.Open();
                string declaracionSql = "SELECT * FROM Declaraciones WHERE Nddtimmioe = @nddtimmioe AND Iddt = @iddt AND nroTransaccion = @nroTransaccion";
                using (var declaracionesComando = new SqlCommand(declaracionSql, conexion))
                {
                    declaracionesComando.Parameters.AddWithValue("@nddtimmioe", nddtimmioe);
                    declaracionesComando.Parameters.AddWithValue("@iddt", iddt);
                    declaracionesComando.Parameters.AddWithValue("@nroTransaccion", NroTransaccion);
                    using (var reader = declaracionesComando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            declaraciones.Iddtextr = reader["Iddtextr"].ToString();
                            declaraciones.Cddtver = reader.GetInt32(reader.GetOrdinal("Cddtver"));
                            declaraciones.Iddtext = reader["Iddtext"].ToString();
                            declaraciones.Iddt = reader["Iddt"].ToString();
                            declaraciones.Iext = reader["Iext"].ToString();
                            declaraciones.Cddteta = reader["Cddteta"].ToString();
                            declaraciones.Dddtoficia = reader.GetDateTime(reader.GetOrdinal("Dddtoficia"));
                            declaraciones.Dddtrectifa = reader.IsDBNull(reader.GetOrdinal("Dddtrectifa"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtrectifa"));
                            declaraciones.Cddtcirvis = reader["Cddtcirvis"].ToString();
                            declaraciones.Qddttaxchg = reader.GetDecimal(reader.GetOrdinal("Qddttaxchg"));
                            declaraciones.Ista = reader["Ista"].ToString();
                            declaraciones.Cddtbur = reader["Cddtbur"].ToString();
                            declaraciones.Cddtburdst = reader["Cddtburdst"].ToString();
                            declaraciones.Cddtdep = reader["Cddtdep"].ToString();
                            declaraciones.Cddtentrep = reader["Cddtentrep"].ToString();
                            declaraciones.Cddtage = reader["Cddtage"].ToString();
                            declaraciones.Cddtagr = reader["Cddtagr"].ToString();
                            declaraciones.Lddtagr = reader["Lddtagr"].ToString();
                            declaraciones.Nddtimmioe = reader["Nddtimmioe"].ToString();
                            declaraciones.Lddtnomioe = reader["Lddtnomioe"].ToString();
                            declaraciones.Cddtpayori = reader["Cddtpayori"].ToString();
                            declaraciones.Cddtpaidst = reader["Cddtpaidst"].ToString();
                            declaraciones.Lddtnomfod = reader["Lddtnomfod"].ToString();
                            declaraciones.Cddtincote = reader["Cddtincote"].ToString();
                            declaraciones.Cddtdevfob = reader["Cddtdevfob"].ToString();
                            declaraciones.Cddtdevfle = reader["Cddtdevfle"].ToString();
                            declaraciones.Cddtdevass = reader["Cddtdevass"].ToString();
                            declaraciones.Cddttransp = reader["Cddttransp"].ToString();
                            declaraciones.Cddtmdetrn = reader["Cddtmdetrn"].ToString();
                            declaraciones.Cddtpaytrn = reader["Cddtpaytrn"].ToString();
                            declaraciones.Nddtart = reader.GetInt32(reader.GetOrdinal("Nddtart"));
                            declaraciones.Nddtdelai = reader.IsDBNull(reader.GetOrdinal("Nddtdelai"))
                                ? (int?)null
                                : reader.GetInt32(reader.GetOrdinal("Nddtdelai"));
                            declaraciones.Dddtbae = reader.IsDBNull(reader.GetOrdinal("Dddtbae"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtbae"));
                            declaraciones.Dddtsalida = reader.IsDBNull(reader.GetOrdinal("Dddtsalida"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtsalida"));
                            declaraciones.Dddtcancel = reader.IsDBNull(reader.GetOrdinal("Dddtcancel"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtcancel"));
                            declaraciones.Dddtechean = reader.IsDBNull(reader.GetOrdinal("Dddtechean"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dddtechean"));
                            declaraciones.Cddtobs = reader["Cddtobs"].ToString();
                            declaraciones.NroTransaccion = reader["NroTransaccion"].ToString();
                            declaraciones.FechaAConsultar = reader["FechaAConsultar"].ToString();
                        }
                    }
                }

                string liquidacionSql = "SELECT * FROM Liquidacion WHERE Iliq = @Iliq  AND nroTransaccion = @nroTransaccion"; 
                using (var liquidacionComando = new SqlCommand(liquidacionSql, conexion))
                {
                    liquidacionComando.Parameters.AddWithValue("@Iliq", declaraciones.Iddt);
                    liquidacionComando.Parameters.AddWithValue("@nroTransaccion", declaraciones.NroTransaccion);
                    using (var reader = liquidacionComando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            declaraciones.Liquidacion.Iliq = reader["Iliq"].ToString();
                            declaraciones.Liquidacion.Cliqdop = reader["Cliqdop"].ToString();
                            declaraciones.Liquidacion.Cliqeta = reader["Cliqeta"].ToString();
                            declaraciones.Liquidacion.Mliq = reader.GetDecimal(reader.GetOrdinal("Mliq"));
                            declaraciones.Liquidacion.Mliqgar = reader.GetDecimal(reader.GetOrdinal("Mliqgar"));
                            declaraciones.Liquidacion.Dlippay = reader.IsDBNull(reader.GetOrdinal("Dlippay"))
                                ? (DateTime?)null
                                : reader.GetDateTime(reader.GetOrdinal("Dlippay"));
                            declaraciones.Liquidacion.Clipnomope = reader["Clipnomope"].ToString();
                        }
                    }
                }
                string articulosSQL = "SELECT * FROM Articulos WHERE Iddt = @Iddt  AND nroTransaccion = @nroTransaccion";
                using (var articulosComando = new SqlCommand(articulosSQL, conexion))
                {
                    articulosComando.Parameters.AddWithValue("@Iddt", declaraciones.Iddt);
                    articulosComando.Parameters.AddWithValue("@nroTransaccion", declaraciones.NroTransaccion);
                    using (var reader = articulosComando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var articulo = new Articulos
                            {
                                Iddt = reader["Iddt"].ToString(),
                                Nart = Convert.ToInt32(reader["Nart"]),
                                Carttyp = reader["Carttyp"].ToString(),
                                Codbenef = reader["Codbenef"].ToString(),
                                Cartetamrc = reader["Cartetamrc"].ToString(),
                                Iespnce = reader["Iespnce"].ToString(),
                                Cartdesc = reader["Cartdesc"].ToString(),
                                Cartpayori = reader["Cartpayori"].ToString(),
                                Cartpayacq = reader["Cartpayacq"].ToString(),
                                Cartpayprc = reader["Cartpayprc"].ToString(),
                                Iddtapu = reader["Iddtapu"].ToString(),
                                Nartapu = reader["Nartapu"] as int?,
                                Qartbul = reader["Qartbul"] as decimal?,
                                Martunitar = reader["Martunitar"] as decimal?,
                                Cartuntdcl = reader["Cartuntdcl"].ToString(),
                                Qartuntdcl = reader["Qartuntdcl"] as decimal?,
                                Cartuntest = reader["Cartuntest"].ToString(),
                                Qartuntest = reader["Qartuntest"] as decimal?,
                                Qartkgrbrt = reader["Qartkgrbrt"] as decimal?,
                                Qartkgrnet = reader["Qartkgrnet"] as decimal?,
                                Martfob = Convert.ToDecimal(reader["Martfob"]),
                                Martfobdol = reader["Martfobdol"] as decimal?,
                                Martfle = reader["Martfle"] as decimal?,
                                Martass = reader["Martass"] as decimal?,
                                Martemma = reader["Martemma"] as decimal?,
                                Martfrai = reader["Martfrai"] as decimal?,
                                Martajuinc = reader["Martajuinc"] as decimal?,
                                Martajuded = reader["Martajuded"] as decimal?,
                                Martbasimp = Convert.ToDecimal(reader["Martbasimp"]),
                                LiquidacionArticulos = new List<LiquidacionArticulos>()
                            };

                            string liquidacionArticulosSQL = "SELECT * FROM LiquidacionArticulos WHERE Iliq = @Iliq and Nart = @Nart  AND nroTransaccion = @nroTransaccion";
                            using (var liquidacionConnection = new SqlConnection(enlaceConexion))  
                            {
                                liquidacionConnection.Open();
                                using (var liquidacionComando = new SqlCommand(liquidacionArticulosSQL, liquidacionConnection))
                                {
                                    liquidacionComando.Parameters.AddWithValue("@Iliq", declaraciones.Iddt);
                                    liquidacionComando.Parameters.AddWithValue("@Nart", articulo.Nart);
                                    liquidacionComando.Parameters.AddWithValue("@nroTransaccion", declaraciones.NroTransaccion);
                                    using (var liquidacionReader = liquidacionComando.ExecuteReader())
                                    {
                                        while (liquidacionReader.Read())
                                        {
                                            var Liqarticulo = new LiquidacionArticulos
                                            {
                                                Iliq = liquidacionReader["Iliq"].ToString(),
                                                Nart = Convert.ToInt32(liquidacionReader["Nart"]),
                                                Clqatax = liquidacionReader["Clqatax"].ToString(),
                                                Clqatyp = liquidacionReader["Clqatyp"].ToString(),
                                                Mlqabas = liquidacionReader["Mlqabas"] as decimal?,
                                                Qlqacoefic = liquidacionReader["Qlqacoefic"] as decimal?,
                                                Mlqa = liquidacionReader["Mlqa"] as decimal?
                                            };

                                            articulo.LiquidacionArticulos.Add(Liqarticulo);
                                        }
                                    }
                                }
                            }
                            declaraciones.Articulos.Add(articulo);
                        }
                    }
                }



            }

            return View(declaraciones);
        }

        // METODO PARA MOSTRAR EL SCRIPT DE LA BASE DE DATOS NECESARIA
        public IActionResult InformacionDB()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // METODO UTILIZADO PARA MANEJAR LA API
        public async Task<IActionResult> Index(string fechaConsultar)
        {
            try
            {
                if (!string.IsNullOrEmpty(fechaConsultar))
                {
                    String  isSuccess = await ServicioAPI.ObtenerDatosDeAPI(fechaConsultar);
                    if (isSuccess == "La data fue procesada exitosamente.")
                    {
                        ViewBag.Message = isSuccess;
                    }
                    else
                    {
                        ViewBag.Error = isSuccess;
                    }
                    ViewBag.FechaConsultar = fechaConsultar;
                }

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Un error ha ocurrido al procesar la data de la API.";
                return View();
            }
        }
    }
}
