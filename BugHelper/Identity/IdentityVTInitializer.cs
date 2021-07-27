using BugHelper.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugHelper.Identity
{
    public class IdentityVTInitializer : DropCreateDatabaseIfModelChanges<IdentityDataContext>
    {
        protected override void Seed(IdentityDataContext context)
        {
            RoleStore<IdentityRole> roleStore = new RoleStore<IdentityRole>(context);
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(roleStore);
            roleManager.Create(new IdentityRole("admin"));
            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(userStore);
            ApplicationUser admin = new ApplicationUser();
            admin.UserName = "admin";
            admin.KatilmaTarihi = System.DateTime.Now;
            admin.LockoutEnabled = false;
            admin.Email = "bughelperteam@gmail.com";
            userManager.Create(admin, "bughelpeR1@");
            userManager.AddToRole(admin.Id, "admin");
            context.SaveChanges();
            base.Seed(context);
            
        }

    }
}