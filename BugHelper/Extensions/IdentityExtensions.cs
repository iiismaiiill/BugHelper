using System.Security.Claims;
using System.Security.Principal;

namespace BugHelper.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetAd(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Ad");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetSoyad(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Soyad");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetPath(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Path");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}