using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BugHelper.Identity
{
    public class CustomPasswordValidator:PasswordValidator
    {
        public override async Task<IdentityResult> ValidateAsync(string password)
        {
            var result = await base.ValidateAsync(password);
            if (password.Contains("12")) //örnek
            {
                var errors = result.Errors.ToList();
                errors.Add("Parola ardışık sayısal ifade içeremez");
                result = new IdentityResult(errors);

            }
            if (!password.Any(char.IsUpper) ||
            !password.Any(char.IsLower) ||
            !password.Any(char.IsDigit)) { 
                var errors = result.Errors.ToList();
                errors.Add("Parola en az bir büyük, bir küçük harf ve en az 1 sayı içermelidir");
                result = new IdentityResult(errors);
            }
            return result;
        }
    }
}