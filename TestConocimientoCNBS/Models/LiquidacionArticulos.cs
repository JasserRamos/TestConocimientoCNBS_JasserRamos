using System;

namespace TestConocimientoCNBS.Models
{
    public class LiquidacionArticulos
    {
        public string Iliq { get; set; }
        public int Nart { get; set; }
        public string Clqatax { get; set; }
        public string Clqatyp { get; set; }
        public decimal? Mlqabas { get; set; }
        public decimal? Qlqacoefic { get; set; }
        public decimal? Mlqa { get; set; }

        public string NroTransaccion { get; set; } 
        public DateTime FechaHoraTrn { get; set; }  
        public string FechaAConsultar { get; set; }  
        public override string ToString()
        {
            return $"Iliq: {Iliq}, Nart: {Nart}, Clqatax: {Clqatax}, Clqatyp: {Clqatyp}, Mlqabas: {Mlqabas}, Qlqacoefic: {Qlqacoefic}, Mlqa: {Mlqa}, NroTransaccion: {NroTransaccion}, FechaHoraTrn: {FechaHoraTrn}, FechaAConsultar: {FechaAConsultar}";
        }
    }
}
