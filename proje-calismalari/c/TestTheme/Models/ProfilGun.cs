using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TestTheme.Models
{
    public class ProfilGun
    {
        public string ad { get; set; }
        public string soyad { get; set; }
        public string istanimi { get; set; }
        public Ulkeler ulke { get; set; }
        public string facebook { get; set; }
        public string twitter { get; set; }
        public string github { get; set; }
        public string bitbucket { get; set; }
        
    }
    public enum Ulkeler
    {
        Türkiye,
        Almanya,
        İngiltere,
        ABD
    }
}