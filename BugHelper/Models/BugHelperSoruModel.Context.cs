﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BugHelper.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class sorularDatabaseEntities1 : DbContext
    {
        public sorularDatabaseEntities1()
            : base("name=sorularDatabaseEntities1")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__MigrationHistory> C__MigrationHistory { get; set; }
        public virtual DbSet<ArtiOys> ArtiOys { get; set; }
        public virtual DbSet<CevaplarModels> CevaplarModels { get; set; }
        public virtual DbSet<EksiOys> EksiOys { get; set; }
        public virtual DbSet<Etikets> Etikets { get; set; }
        public virtual DbSet<SorularModels> SorularModels { get; set; }
    }
}
