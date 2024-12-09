using System;
namespace TestConocimientoCNBS.Models
{
    public class Liquidacion
    {
        public string Iliq { get; set; }
        public string Cliqdop { get; set; }
        public string Cliqeta { get; set; }
        public decimal Mliq { get; set; }
        public decimal Mliqgar { get; set; }
        public DateTime? Dlippay { get; set; }
        public string Clipnomope { get; set; }
        public string NroTransaccion { get; set; }
        public DateTime FechaHoraTrn { get; set; }
        public string FechaAConsultar { get; set; }

        public override string ToString()
        {
            return $"Iliq: {Iliq}, Cliqdop: {Cliqdop}, Cliqeta: {Cliqeta}, Mliq: {Mliq}, Mliqgar: {Mliqgar}, Dlippay: {Dlippay}, Clipnomope: {Clipnomope}";
        }
    }
}
