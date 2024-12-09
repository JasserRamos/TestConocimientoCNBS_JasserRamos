using System;

namespace TestConocimientoCNBS.Models
{
    public class Declaraciones
    {
        public string Iddtextr { get; set; }
        public int Cddtver { get; set; }
        public string Iddtext { get; set; }
        public string Iddt { get; set; }
        public string Iext { get; set; }
        public string Cddteta { get; set; }
        public DateTime Dddtoficia { get; set; }
        public DateTime? Dddtrectifa { get; set; }
        public string Cddtcirvis { get; set; }
        public decimal Qddttaxchg { get; set; }
        public string Ista { get; set; }
        public string Cddtbur { get; set; }
        public string Cddtburdst { get; set; }
        public string Cddtdep { get; set; }
        public string Cddtentrep { get; set; }
        public string Cddtage { get; set; }
        public string Cddtagr { get; set; }
        public string Lddtagr { get; set; }
        public string Nddtimmioe { get; set; }
        public string Lddtnomioe { get; set; }
        public string Cddtpayori { get; set; }
        public string Cddtpaidst { get; set; }
        public string Lddtnomfod { get; set; }
        public string Cddtincote { get; set; }
        public string Cddtdevfob { get; set; }
        public string Cddtdevfle { get; set; }
        public string Cddtdevass { get; set; }
        public string Cddttransp { get; set; }
        public string Cddtmdetrn { get; set; }
        public string Cddtpaytrn { get; set; }
        public int Nddtart { get; set; }
        public int? Nddtdelai { get; set; }
        public DateTime? Dddtbae { get; set; }
        public DateTime? Dddtsalida { get; set; }
        public DateTime? Dddtcancel { get; set; }
        public DateTime? Dddtechean { get; set; }
        public string Cddtobs { get; set; }

        public Liquidacion Liquidacion { get; set; }
        public ICollection<Articulos> Articulos { get; set; }
        public string NroTransaccion { get; set; }
        public DateTime FechaHoraTrn { get; set; }
        public string FechaAConsultar { get; set; }
        public Declaraciones()
        {
            Articulos = new List<Articulos>();
            Liquidacion = new Liquidacion();
        }
        public override string ToString()
        {
            return $"Iddtextr: {Iddtextr}, Cddtver: {Cddtver}, Iddtext: {Iddtext}, Iddt: {Iddt}, Iext: {Iext}, Cddteta: {Cddteta}, Dddtoficia: {Dddtoficia}, Dddtrectifa: {Dddtrectifa}, Cddtcirvis: {Cddtcirvis}, Qddttaxchg: {Qddttaxchg}, Ista: {Ista}, Cddtbur: {Cddtbur}, Cddtburdst: {Cddtburdst}, Cddtdep: {Cddtdep}, Cddtentrep: {Cddtentrep}, Cddtage: {Cddtage}, Cddtagr: {Cddtagr}, Lddtagr: {Lddtagr}, Nddtimmioe: {Nddtimmioe}, Lddtnomioe: {Lddtnomioe}, Cddtpayori: {Cddtpayori}, Cddtpaidst: {Cddtpaidst}, Lddtnomfod: {Lddtnomfod}, Cddtincote: {Cddtincote}, Cddtdevfob: {Cddtdevfob}, Cddtdevfle: {Cddtdevfle}, Cddtdevass: {Cddtdevass}, Cddttransp: {Cddttransp}, Cddtmdetrn: {Cddtmdetrn}, Cddtpaytrn: {Cddtpaytrn}, Nddtart: {Nddtart}, Nddtdelai: {Nddtdelai}, Dddtbae: {Dddtbae}, Dddtsalida: {Dddtsalida}, Dddtcancel: {Dddtcancel}, Dddtechean: {Dddtechean}, Cddtobs: {Cddtobs}";
        }
    }
}
