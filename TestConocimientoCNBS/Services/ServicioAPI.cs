using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using TestConocimientoCNBS.Models;
using Newtonsoft.Json;
using DotNetEnv;
using Microsoft.Data.SqlClient;

namespace TestConocimientoCNBS.Services
{
    public class ServicioAPI
    {
        static ServicioAPI()
        {
            Env.Load();
        }
        
        public class EstructuraJsonDeclaraciones
        {
            public List<Articulos> ART { get; set; }
            public Declaraciones DDT { get; set; }
            public Liquidacion LIQ { get; set; }
            public List<LiquidacionArticulos> LQA { get; set; }
        }
        // METODO PARA DESCOMPRIMIR LOS DATOS DE DATOSCOMPRIMIDOS
        public static async System.Threading.Tasks.Task<string> DecompressAsync(string compressedString)
        {
            try
            {
                using (MemoryStream msi = new MemoryStream(Convert.FromBase64String(compressedString)))
                using (MemoryStream mso = new MemoryStream())
                {
                    using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                    {
                        await gs.CopyToAsync(mso);
                    }
                    return Encoding.UTF8.GetString(mso.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        // METODO PARA OBTENER DATOS DE LA API
        public static async Task<String> ObtenerDatosDeAPI(string fechaConsultar)
        {
            try
            {
                HttpClient ClienteHttp = new HttpClient();
                string llave = Env.GetString("API_KEY");
                string apiURL = Env.GetString("API_URL");
                if (string.IsNullOrEmpty(llave) || string.IsNullOrEmpty(apiURL))
                {
                    throw new Exception("API_KEY o API_URL no encontrados en el archivo .env.");
                }
                // API URL
                string url = $"{apiURL}?Fecha={fechaConsultar}";

                ClienteHttp.DefaultRequestHeaders.Add("ApiKey", llave);
                HttpResponseMessage response = await ClienteHttp.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    // LEER LA RESPUESTA XML
                    string xmlContent = await response.Content.ReadAsStringAsync();

                    // PARSEAR EL XML PARA EXTRAER DATOS COMPRIMIDOS
                    XElement xmlDoc = XElement.Parse(xmlContent);
                    string datosComprimidos = xmlDoc.Descendants("datosComprimidos").FirstOrDefault()?.Value;
                     
                    if (!string.IsNullOrEmpty(datosComprimidos))
                    {
                        string nroTransaccion = xmlDoc.Attribute("nroTransaccion")?.Value;
                        string fechaHoraTrn = xmlDoc.Attribute("fechaHoraTrn")?.Value;
                        DateTime fechaHoraTrnDateTime = DateTime.Parse(fechaHoraTrn);
                        string fechaAConsultar = xmlDoc.Element("fechaAConsultar")?.Value;
                        // SE UTLIZA LA FUNCION DecompressAsync PARA EL MANEJO DE DATOS COMPRIMIDOS
                        string jsonContent = await DecompressAsync(datosComprimidos);
                        
                        var deserializedObject = JsonConvert.DeserializeObject<object>(jsonContent);
                        List<EstructuraJsonDeclaraciones> estructuras = JsonConvert.DeserializeObject<List<EstructuraJsonDeclaraciones>>(jsonContent);
                        foreach (var estructura in estructuras)
                        {
                            // METODO PARA INSERTAR LA DATA EN LA BASE DE DATOS
                            await InsertarDatosEnBaseDeDatos(estructura, nroTransaccion, fechaHoraTrnDateTime, fechaAConsultar);
                        }
                        return "La data fue procesada exitosamente.";
                    }
                    else
                    {
                        Console.WriteLine("No se encontraron datos comprimidos.");
                        return "No se encuentran los datos comprimidos.";
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return $"{response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return "Esta fecha ya fue consultada.";
            }
        }
        //METODO PARA INSERTAR LOS DATOS EN LA BASE DE DATOS
        private static async Task InsertarDatosEnBaseDeDatos(EstructuraJsonDeclaraciones estructura, String nroTransaccion, DateTime fechaHoraTrn, String fechaAConsultar)
        {
            string enlaceConexion = Env.GetString("DB_CONNECTION_STRING");

            await using (SqlConnection conexion = new SqlConnection(enlaceConexion))
            {
                await conexion.OpenAsync();
                string insertarConsultaLiquidacion = @"
            INSERT INTO Liquidacion (Iliq, Cliqdop, Cliqeta, Mliq, Mliqgar, Dlippay, Clipnomope,nroTransaccion,fechaHoraTrn,fechaAConsultar)
            VALUES (@Iliq, @Cliqdop, @Cliqeta, @Mliq, @Mliqgar, @Dlippay, @Clipnomope,@nroTransaccion,@fechaHoraTrn,@fechaAConsultar)";
                await using (SqlCommand cmd = new SqlCommand(insertarConsultaLiquidacion, conexion))
                {
                        cmd.Parameters.AddWithValue("@Iliq", estructura.LIQ.Iliq);
                        cmd.Parameters.AddWithValue("@Cliqdop", estructura.LIQ.Cliqdop);
                        cmd.Parameters.AddWithValue("@Cliqeta", estructura.LIQ.Cliqeta);
                        cmd.Parameters.AddWithValue("@Mliq", estructura.LIQ.Mliq);
                        cmd.Parameters.AddWithValue("@Mliqgar", estructura.LIQ.Mliqgar);
                        cmd.Parameters.AddWithValue("@Dlippay", estructura.LIQ.Dlippay.HasValue ? estructura.LIQ.Dlippay.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Clipnomope", string.IsNullOrEmpty(estructura.LIQ.Clipnomope) ? DBNull.Value : estructura.LIQ.Clipnomope);
                        cmd.Parameters.AddWithValue("@nroTransaccion", nroTransaccion);
                        cmd.Parameters.AddWithValue("@fechaHoraTrn", fechaHoraTrn);
                        cmd.Parameters.AddWithValue("@fechaAConsultar", fechaAConsultar);

                        await cmd.ExecuteNonQueryAsync();
                }
                    string insertarConsultaDeclaraciones = @"
            INSERT INTO Declaraciones (
                Iddtextr, Cddtver, Iddtext, Iddt, Iext, Cddteta, 
                Dddtoficia, Dddtrectifa, Cddtcirvis, Qddttaxchg, 
                Ista, Cddtbur, Cddtburdst, Cddtdep, Cddtentrep, 
                Cddtage, Cddtagr, Lddtagr, Nddtimmioe, Lddtnomioe, 
                Cddtpayori, Cddtpaidst, Lddtnomfod, Cddtincote, 
                Cddtdevfob, Cddtdevfle, Cddtdevass, Cddttransp, 
                Cddtmdetrn, Cddtpaytrn, Nddtart, Nddtdelai, 
                Dddtbae, Dddtsalida, Dddtcancel, Dddtechean, Cddtobs,nroTransaccion,fechaHoraTrn,fechaAConsultar)
            VALUES (
                @Iddtextr, @Cddtver, @Iddtext, @Iddt, @Iext, @Cddteta, 
                @Dddtoficia, @Dddtrectifa, @Cddtcirvis, @Qddttaxchg, 
                @Ista, @Cddtbur, @Cddtburdst, @Cddtdep, @Cddtentrep, 
                @Cddtage, @Cddtagr, @Lddtagr, @Nddtimmioe, @Lddtnomioe, 
                @Cddtpayori, @Cddtpaidst, @Lddtnomfod, @Cddtincote, 
                @Cddtdevfob, @Cddtdevfle, @Cddtdevass, @Cddttransp, 
                @Cddtmdetrn, @Cddtpaytrn, @Nddtart, @Nddtdelai, 
                @Dddtbae, @Dddtsalida, @Dddtcancel, @Dddtechean, @Cddtobs,@nroTransaccion,@fechaHoraTrn,@fechaAConsultar)";

                    await using (SqlCommand cmd = new SqlCommand(insertarConsultaDeclaraciones, conexion))
                    {
                        // Add parameters with null handling
                        cmd.Parameters.AddWithValue("@Iddtextr", estructura.DDT.Iddtextr);
                        cmd.Parameters.AddWithValue("@Cddtver", estructura.DDT.Cddtver);
                        cmd.Parameters.AddWithValue("@Iddtext", estructura.DDT.Iddtext);
                        cmd.Parameters.AddWithValue("@Iddt", estructura.DDT.Iddt);
                        cmd.Parameters.AddWithValue("@Iext", string.IsNullOrEmpty(estructura.DDT.Iext) ? DBNull.Value : estructura.DDT.Iext);
                        cmd.Parameters.AddWithValue("@Cddteta", estructura.DDT.Cddteta);
                        cmd.Parameters.AddWithValue("@Dddtoficia", estructura.DDT.Dddtoficia);
                        cmd.Parameters.AddWithValue("@Dddtrectifa", estructura.DDT.Dddtrectifa ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cddtcirvis", string.IsNullOrEmpty(estructura.DDT.Cddtcirvis) ? DBNull.Value : estructura.DDT.Cddtcirvis);
                        cmd.Parameters.AddWithValue("@Qddttaxchg", estructura.DDT.Qddttaxchg);
                        cmd.Parameters.AddWithValue("@Ista", estructura.DDT.Ista);
                        cmd.Parameters.AddWithValue("@Cddtbur", estructura.DDT.Cddtbur);
                        cmd.Parameters.AddWithValue("@Cddtburdst", string.IsNullOrEmpty(estructura.DDT.Cddtburdst) ? DBNull.Value : estructura.DDT.Cddtburdst);
                        cmd.Parameters.AddWithValue("@Cddtdep", string.IsNullOrEmpty(estructura.DDT.Cddtdep) ? DBNull.Value : estructura.DDT.Cddtdep);
                        cmd.Parameters.AddWithValue("@Cddtentrep", string.IsNullOrEmpty(estructura.DDT.Cddtentrep) ? DBNull.Value : estructura.DDT.Cddtentrep);
                        cmd.Parameters.AddWithValue("@Cddtage", estructura.DDT.Cddtage);
                        cmd.Parameters.AddWithValue("@Cddtagr", string.IsNullOrEmpty(estructura.DDT.Cddtagr) ? DBNull.Value : estructura.DDT.Cddtagr);
                        cmd.Parameters.AddWithValue("@Lddtagr", string.IsNullOrEmpty(estructura.DDT.Lddtagr) ? DBNull.Value : estructura.DDT.Lddtagr);
                        cmd.Parameters.AddWithValue("@Nddtimmioe", estructura.DDT.Nddtimmioe);
                        cmd.Parameters.AddWithValue("@Lddtnomioe", estructura.DDT.Lddtnomioe);
                        cmd.Parameters.AddWithValue("@Cddtpayori", string.IsNullOrEmpty(estructura.DDT.Cddtpayori) ? DBNull.Value : estructura.DDT.Cddtpayori);
                        cmd.Parameters.AddWithValue("@Cddtpaidst", string.IsNullOrEmpty(estructura.DDT.Cddtpaidst) ? DBNull.Value : estructura.DDT.Cddtpaidst);
                        cmd.Parameters.AddWithValue("@Lddtnomfod", string.IsNullOrEmpty(estructura.DDT.Lddtnomfod) ? DBNull.Value : estructura.DDT.Lddtnomfod);
                        cmd.Parameters.AddWithValue("@Cddtincote", string.IsNullOrEmpty(estructura.DDT.Cddtincote) ? DBNull.Value : estructura.DDT.Cddtincote);
                        cmd.Parameters.AddWithValue("@Cddtdevfob", estructura.DDT.Cddtdevfob);
                        cmd.Parameters.AddWithValue("@Cddtdevfle", string.IsNullOrEmpty(estructura.DDT.Cddtdevfle) ? DBNull.Value : estructura.DDT.Cddtdevfle);
                        cmd.Parameters.AddWithValue("@Cddtdevass", string.IsNullOrEmpty(estructura.DDT.Cddtdevass) ? DBNull.Value : estructura.DDT.Cddtdevass);
                        cmd.Parameters.AddWithValue("@Cddttransp", string.IsNullOrEmpty(estructura.DDT.Cddttransp) ? DBNull.Value : estructura.DDT.Cddttransp);
                        cmd.Parameters.AddWithValue("@Cddtmdetrn", string.IsNullOrEmpty(estructura.DDT.Cddtmdetrn) ? DBNull.Value : estructura.DDT.Cddtmdetrn);
                        cmd.Parameters.AddWithValue("@Cddtpaytrn", string.IsNullOrEmpty(estructura.DDT.Cddtpaytrn) ? DBNull.Value : estructura.DDT.Cddtpaytrn);
                        cmd.Parameters.AddWithValue("@Nddtart", estructura.DDT.Nddtart);
                        cmd.Parameters.AddWithValue("@Nddtdelai", estructura.DDT.Nddtdelai ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Dddtbae", estructura.DDT.Dddtbae ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Dddtsalida", estructura.DDT.Dddtsalida ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Dddtcancel", estructura.DDT.Dddtcancel ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Dddtechean", estructura.DDT.Dddtechean ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Cddtobs", string.IsNullOrEmpty(estructura.DDT.Cddtobs) ? DBNull.Value : estructura.DDT.Cddtobs);
                        cmd.Parameters.AddWithValue("@nroTransaccion", nroTransaccion);
                        cmd.Parameters.AddWithValue("@fechaHoraTrn", fechaHoraTrn);
                        cmd.Parameters.AddWithValue("@fechaAConsultar", fechaAConsultar);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // INSERCION DE ARTICULOS
                string insertarConsultaArticulos = @"
                    INSERT INTO Articulos (Iddt, Nart, Carttyp, Codbenef, Cartetamrc, Iespnce, Cartdesc, Cartpayori, Cartpayacq, Cartpayprc, Iddtapu, Nartapu, Qartbul, Martunitar, Cartuntdcl, Qartuntdcl, Cartuntest, Qartuntest, Qartkgrbrt, Qartkgrnet, Martfob, Martfobdol, Martfle, Martass, Martemma, Martfrai, Martajuinc, Martajuded, Martbasimp,nroTransaccion,fechaHoraTrn,fechaAConsultar)
                    VALUES (@Iddt, @Nart, @Carttyp, @Codbenef, @Cartetamrc, @Iespnce, @Cartdesc, @Cartpayori, @Cartpayacq, @Cartpayprc, @Iddtapu, @Nartapu, @Qartbul, @Martunitar, @Cartuntdcl, @Qartuntdcl, @Cartuntest, @Qartuntest, @Qartkgrbrt, @Qartkgrnet, @Martfob, @Martfobdol, @Martfle, @Martass, @Martemma, @Martfrai, @Martajuinc, @Martajuded, @Martbasimp ,@nroTransaccion,@fechaHoraTrn,@fechaAConsultar)";

                foreach (var articulo in estructura.ART)
                {
                    try
                    {
                        await using (SqlCommand cmd = new SqlCommand(insertarConsultaArticulos, conexion))
                        {
                            cmd.Parameters.AddWithValue("@Iddt", articulo.Iddt);
                            cmd.Parameters.AddWithValue("@Nart", articulo.Nart);
                            cmd.Parameters.AddWithValue("@Carttyp", articulo.Carttyp);
                            cmd.Parameters.AddWithValue("@Codbenef", articulo.Codbenef);
                            cmd.Parameters.AddWithValue("@Cartetamrc", string.IsNullOrEmpty(articulo.Cartetamrc) ? DBNull.Value : articulo.Cartetamrc);
                            cmd.Parameters.AddWithValue("@Iespnce", articulo.Iespnce);
                            cmd.Parameters.AddWithValue("@Cartdesc", articulo.Cartdesc);
                            cmd.Parameters.AddWithValue("@Cartpayori", string.IsNullOrEmpty(articulo.Cartpayori) ? DBNull.Value : articulo.Cartpayori);
                            cmd.Parameters.AddWithValue("@Cartpayacq", string.IsNullOrEmpty(articulo.Cartpayacq) ? DBNull.Value : articulo.Cartpayacq);
                            cmd.Parameters.AddWithValue("@Cartpayprc", string.IsNullOrEmpty(articulo.Cartpayprc) ? DBNull.Value : articulo.Cartpayprc);
                            cmd.Parameters.AddWithValue("@Iddtapu", string.IsNullOrEmpty(articulo.Iddtapu) ? DBNull.Value : articulo.Iddtapu);
                            cmd.Parameters.AddWithValue("@Nartapu", articulo.Nartapu.HasValue ? articulo.Nartapu.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qartbul", articulo.Qartbul.HasValue ? articulo.Qartbul.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martunitar", articulo.Martunitar.HasValue ? articulo.Martunitar.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cartuntdcl", string.IsNullOrEmpty(articulo.Cartuntdcl) ? DBNull.Value : articulo.Cartuntdcl);
                            cmd.Parameters.AddWithValue("@Qartuntdcl", articulo.Qartuntdcl.HasValue ? articulo.Qartuntdcl.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Cartuntest", string.IsNullOrEmpty(articulo.Cartuntest) ? DBNull.Value : articulo.Cartuntest);
                            cmd.Parameters.AddWithValue("@Qartuntest", articulo.Qartuntest.HasValue ? articulo.Qartuntest.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qartkgrbrt", articulo.Qartkgrbrt.HasValue ? articulo.Qartkgrbrt.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qartkgrnet", articulo.Qartkgrnet.HasValue ? articulo.Qartkgrnet.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martfob", articulo.Martfob);
                            cmd.Parameters.AddWithValue("@Martfobdol", articulo.Martfobdol.HasValue ? articulo.Martfobdol.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martfle", articulo.Martfle.HasValue ? articulo.Martfle.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martass", articulo.Martass.HasValue ? articulo.Martass.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martemma", articulo.Martemma.HasValue ? articulo.Martemma.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martfrai", articulo.Martfrai.HasValue ? articulo.Martfrai.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martajuinc", articulo.Martajuinc.HasValue ? articulo.Martajuinc.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martajuded", articulo.Martajuded.HasValue ? articulo.Martajuded.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Martbasimp", articulo.Martbasimp);
                            cmd.Parameters.AddWithValue("@nroTransaccion", nroTransaccion);
                            cmd.Parameters.AddWithValue("@fechaHoraTrn", fechaHoraTrn);
                            cmd.Parameters.AddWithValue("@fechaAConsultar", fechaAConsultar);


                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        Console.WriteLine($"Error SQL: {sqlEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error inesperado: {ex.Message}");
                    }
                }




                // INCERSION DE LIQUIDACION DE ARTICULOS
                string insertarConsultaLiquidacionArticulos = @"
                    INSERT INTO LiquidacionArticulos (Iliq, Nart, Clqatax, Clqatyp, Mlqabas, Qlqacoefic, Mlqa,nroTransaccion,fechaHoraTrn,fechaAConsultar)
                    VALUES (@Iliq, @Nart, @Clqatax, @Clqatyp, @Mlqabas, @Qlqacoefic, @Mlqa,@nroTransaccion,@fechaHoraTrn,@fechaAConsultar)";
                string insertarConsultaLiquidacionArticulosSinArticulos = @"
                    INSERT INTO LiquidacionArticulosSinArticulos (Iliq, Nart, Clqatax, Clqatyp, Mlqabas, Qlqacoefic, Mlqa, nroTransaccion, fechaHoraTrn, fechaAConsultar, descripcionError)
                    VALUES (@Iliq, @Nart, @Clqatax, @Clqatyp, @Mlqabas, @Qlqacoefic, @Mlqa, @nroTransaccion, @fechaHoraTrn, @fechaAConsultar, @descripcionError)";

                foreach (var liquidacionArticulo in estructura.LQA)
                {


                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(insertarConsultaLiquidacionArticulos, conexion))
                        {
                            cmd.Parameters.AddWithValue("@Iliq", liquidacionArticulo.Iliq);
                            cmd.Parameters.AddWithValue("@Nart", liquidacionArticulo.Nart);
                            cmd.Parameters.AddWithValue("@Clqatax", liquidacionArticulo.Clqatax);
                            cmd.Parameters.AddWithValue("@Clqatyp", liquidacionArticulo.Clqatyp);
                            cmd.Parameters.AddWithValue("@Mlqabas", liquidacionArticulo.Mlqabas.HasValue ? liquidacionArticulo.Mlqabas.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Qlqacoefic", liquidacionArticulo.Qlqacoefic.HasValue ? liquidacionArticulo.Qlqacoefic.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Mlqa", liquidacionArticulo.Mlqa.HasValue ? liquidacionArticulo.Mlqa.Value : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@nroTransaccion", nroTransaccion);
                            cmd.Parameters.AddWithValue("@fechaHoraTrn", fechaHoraTrn);
                            cmd.Parameters.AddWithValue("@fechaAConsultar", fechaAConsultar);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        Console.WriteLine($"Error SQL al insertar registro DE LIQUIDACION DE ARTICULO Iliq {liquidacionArticulo.Iliq}, Nart {liquidacionArticulo.Nart}: {sqlEx.Message}");
                        try
                        {
                            using (SqlCommand cmdError = new SqlCommand(insertarConsultaLiquidacionArticulosSinArticulos, conexion))
                            {
                                cmdError.Parameters.AddWithValue("@Iliq", liquidacionArticulo.Iliq);
                                cmdError.Parameters.AddWithValue("@Nart", liquidacionArticulo.Nart);
                                cmdError.Parameters.AddWithValue("@Clqatax", liquidacionArticulo.Clqatax);
                                cmdError.Parameters.AddWithValue("@Clqatyp", liquidacionArticulo.Clqatyp);
                                cmdError.Parameters.AddWithValue("@Mlqabas", liquidacionArticulo.Mlqabas.HasValue ? liquidacionArticulo.Mlqabas.Value : (object)DBNull.Value);
                                cmdError.Parameters.AddWithValue("@Qlqacoefic", liquidacionArticulo.Qlqacoefic.HasValue ? liquidacionArticulo.Qlqacoefic.Value : (object)DBNull.Value);
                                cmdError.Parameters.AddWithValue("@Mlqa", liquidacionArticulo.Mlqa.HasValue ? liquidacionArticulo.Mlqa.Value : (object)DBNull.Value);
                                cmdError.Parameters.AddWithValue("@nroTransaccion", nroTransaccion);
                                cmdError.Parameters.AddWithValue("@fechaHoraTrn", fechaHoraTrn);
                                cmdError.Parameters.AddWithValue("@fechaAConsultar", fechaAConsultar);
                                cmdError.Parameters.AddWithValue("@descripcionError", sqlEx.Message);

                                await cmdError.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception insertErrorEx)
                        {
                            Console.WriteLine($"Error al insertar en LiquidacionArticulosSinArticulos: {insertErrorEx.Message}");
                        }
                    }

                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error general al insertar registro Iliq {liquidacionArticulo.Iliq}, Nart {liquidacionArticulo.Nart}: {ex.Message}");
                    }
                }

            }
        }
    }
}
