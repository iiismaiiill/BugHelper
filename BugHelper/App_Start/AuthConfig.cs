using DotNetOpenAuth.GoogleOAuth2;
using DotNetOpenAuth.FacebookOAuth2;
using Microsoft.AspNet.Membership.OpenAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Owin.Security.Facebook;

namespace BugHelper.App_Start
{
    public class AuthConfig
    {
        public static void RegisterAuth()
        {
            GoogleOAuth2Client clientGoog = new GoogleOAuth2Client("624818846515-v5b6s4j55celu9b85nrh2eo540uol03j.apps.googleusercontent.com", "gsxuFDXgVvdUPuALkzD_w3c9");
            IDictionary<string, string> extraData = new Dictionary<string, string>();
            OpenAuth.AuthenticationClients.Add("google", () => clientGoog, extraData);
            OpenAuth.AuthenticationClients.Add("facebook",() => new FacebookClient("482902229248068", "ca2b82c3e03570af0d8c7cecdce3944a"));
        }
    }
}