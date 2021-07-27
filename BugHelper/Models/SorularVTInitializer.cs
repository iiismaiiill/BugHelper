using BugHelper.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class SorularVTInitializer: DropCreateDatabaseIfModelChanges<SorularContext>
    {
        protected override void Seed(SorularContext context)
        {
            base.Seed(context);
        }

    }
}