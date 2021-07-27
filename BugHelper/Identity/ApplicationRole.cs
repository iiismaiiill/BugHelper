using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugHelper.Identity
{
    public class ApplicationRole:IdentityRole
    {
        public String MyProperty { get; set; }
    }
}