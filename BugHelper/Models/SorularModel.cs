using BugHelper.Identity;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BugHelper.Models
{
    public class SorularModel
    {
        public SorularModel()
        {
            Cevaplar = new List<CevaplarModel>();
            ArtiOylar = new List<ArtiOy>();
            EksiOylar = new List<EksiOy>();
        }
        public int Id { get; set; }
        public string SoruIcerik { get; set; }
        [Required]
        public string SoruBaslik { get; set; }
        public DateTime SorulmaTarihi { get; set; }
        public string SoruSahibi { get; set; }
        public string SoruSahibiPath { get; set; }
        public bool Onay { get; set; }
        public int TiklanmaSayisi { get; set; }
        public int CevapSayisi { get; set; }
        public int Deger { get; set; }
        public string KodlamaDili { get; set; }
        public List<CevaplarModel> Cevaplar { get; set; }
        public List<ArtiOy> ArtiOylar { get; set; }
        public List<EksiOy> EksiOylar { get; set; }

        public static implicit operator SorularModel(CevaplarModel v)
        {
            throw new NotImplementedException();
        }
    }
    public class ArtiOy
    {
        public int Id { get; set; }

        public string ArtiOySahibi { get; set; }
        public SorularModel Soru { get; set; }
    }
    public class ArtiOyCevaplar
    {
        public int Id { get; set; }

        public string ArtiOySahibi { get; set; }
        public CevaplarModel Cevap { get; set; }
    }
    public class EksiOy
    {
        public int Id { get; set; }

        public string EksiOySahibi { get; set; }
        public SorularModel Soru { get; set; }

    }
    public class EksiOyCevaplar
    {
        public int Id { get; set; }

        public string EksiOySahibi { get; set; }
        public CevaplarModel Cevap { get; set; }

    }
    public class Etiket
    {
        public int Id { get; set; }
        public string KodlamaDili { get; set; }
    }
    public class SorularPaged
    {
        public List<SorularModel> Sorular { get; set; }
        public List<EtiketListesi> Etiketler { get; set; }
        public ApplicationUser SoruIzleyici { get; set; }
        public string[] EtiketRenkleri { get; set; }
        public int SoruSayisi { get; set; }
        public IPagedList PagedList { get; set; }
        public class EtiketListesi
        {
             public int SoruSayisi { get; set; }
             public string Etiket { get; set; }
        }
    }
}