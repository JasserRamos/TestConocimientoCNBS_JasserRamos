using System;
namespace TestConocimientoCNBS.Models
{
    public class Articulos
    {
        public string Iddt { get; set; }
        public int Nart { get; set; }
        public string Carttyp { get; set; }
        public string Codbenef { get; set; }
        public string Cartetamrc { get; set; }
        public string Iespnce { get; set; }
        public string Cartdesc { get; set; }
        public string Cartpayori { get; set; }
        public string Cartpayacq { get; set; }
        public string Cartpayprc { get; set; }
        public string Iddtapu { get; set; }
        public int? Nartapu { get; set; }
        public decimal? Qartbul { get; set; }
        public decimal? Martunitar { get; set; }
        public string Cartuntdcl { get; set; }
        public decimal? Qartuntdcl { get; set; }
        public string Cartuntest { get; set; }
        public decimal? Qartuntest { get; set; }
        public decimal? Qartkgrbrt { get; set; }
        public decimal? Qartkgrnet { get; set; }
        public decimal Martfob { get; set; }
        public decimal? Martfobdol { get; set; }
        public decimal? Martfle { get; set; }
        public decimal? Martass { get; set; }
        public decimal? Martemma { get; set; }
        public decimal? Martfrai { get; set; }
        public decimal? Martajuinc { get; set; }
        public decimal? Martajuded { get; set; }
        public decimal Martbasimp { get; set; }
        public string NroTransaccion { get; set; }
        public DateTime FechaHoraTrn { get; set; }
        public string FechaAConsultar { get; set; }
        public ICollection<LiquidacionArticulos> LiquidacionArticulos { get; set; }
        public override string ToString()
        {
            return $"Iddt: {Iddt}, Nart: {Nart}, Carttyp: {Carttyp}, Codbenef: {Codbenef}, Cartetamrc: {Cartetamrc}, Iespnce: {Iespnce}, Cartdesc: {Cartdesc}, Cartpayori: {Cartpayori}, Cartpayacq: {Cartpayacq}, Cartpayprc: {Cartpayprc}, Iddtapu: {Iddtapu}, Nartapu: {Nartapu}, Qartbul: {Qartbul}, Martunitar: {Martunitar}, Cartuntdcl: {Cartuntdcl}, Qartuntdcl: {Qartuntdcl}, Cartuntest: {Cartuntest}, Qartuntest: {Qartuntest}, Qartkgrbrt: {Qartkgrbrt}, Qartkgrnet: {Qartkgrnet}, Martfob: {Martfob}, Martfobdol: {Martfobdol}, Martfle: {Martfle}, Martass: {Martass}, Martemma: {Martemma}, Martfrai: {Martfrai}, Martajuinc: {Martajuinc}, Martajuded: {Martajuded}, Martbasimp: {Martbasimp}";
        }
    }
}
