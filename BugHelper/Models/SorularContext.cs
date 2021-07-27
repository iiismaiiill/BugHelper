using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class SorularContext:DbContext
    {
        public SorularContext() : base("sorularConnection")
        {
            Database.SetInitializer(new SorularVTInitializer());
        }
        public DbSet<SorularModel> Sorular { get; set; }
        public DbSet<CevaplarModel> Cevaplar { get; set; }
        public DbSet<ArtiOy> ArtiOy { get; set; }
        public DbSet<ArtiOyCevaplar> ArtiOyCevaplar { get; set; }
        public DbSet<EksiOy> EksiOy { get; set; }
        public DbSet<EksiOyCevaplar> EksiOyCevaplar { get; set; }
        public DbSet<Etiket> Etiketler { get; set; }
        public DbSet<YeniHaberler> YeniHaberler { get; set; }
    }
}