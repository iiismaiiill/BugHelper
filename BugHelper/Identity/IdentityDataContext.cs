using BugHelper.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace BugHelper.Identity
{
    public class IdentityDataContext:IdentityDbContext<ApplicationUser>
    {
        public IdentityDataContext() : base("identityConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer(new IdentityVTInitializer());
        }
        public static IdentityDataContext Create()
        {
            return new IdentityDataContext();
        }
        public DbSet<TakipciModel> Takipci { get; set; }
        public DbSet<TakipEttikleriModel> TakipEttikleri { get; set; }
        public DbSet<FavoriSorularModel> FavoriSorular { get; set; }
        public DbSet<IletisimModel> Iletisim { get; set; }
        public DbSet<UlkelerModel> Ulkeler { get; set; }

    }
}