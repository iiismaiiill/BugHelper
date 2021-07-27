using System;
using System.Collections.Generic;

namespace BugHelper.Models
{
    public class CevaplarModel
    {
        public CevaplarModel()
        {
            ArtiOyCevaplar = new List<ArtiOyCevaplar>();
            EksiOyCevaplar = new List<EksiOyCevaplar>();
        }
        public int Id { get; set; }
        public string Cevap { get; set; }
        public string CevapSahibi { get; set; }
        public string  CevapSahibiPath { get; set; }
        public DateTime CevapTarihi { get; set; }
        public bool Onay { get; set; }
        public int Deger { get; set; }
        public SorularModel Soru { get; set; }
        public List<ArtiOyCevaplar> ArtiOyCevaplar { get; set; }
        public List<EksiOyCevaplar> EksiOyCevaplar { get; set; }
    }
    
}