using BugHelper.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PagedList;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class LoginModel
    {
        [Required]
        public String UserName { get; set; }
        [Required]
        public String Password { get; set; }
    }
    public class Register
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adınızı giriniz")]
        [StringLength(18, ErrorMessage = "Kullanıcı adı en az 2, en fazla 18 karakter uzunluğunda olmalıdır", MinimumLength = 2)]
        public String UserName { get; set; }
        [Required]
        public String Email { get; set; }
        [Required(ErrorMessage = "Parola alanı boş bırakılamaz")]
        [StringLength(18, ErrorMessage = "Parola en az 2, en fazla 18 karakter uzunluğunda olmalıdır", MinimumLength = 8)]
        public String Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Lütfen parolanızı tekrar giriniz")]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor")]
        public String ConfirmPassword { get; set; }
        
    }
    public class ExternalRegister
    {
        [Required(ErrorMessage = "Lütfen kullanıcı adınızı giriniz")]
        [StringLength(18, ErrorMessage = "Kullanıcı adı en az 2, en fazla 18 karakter uzunluğunda olmalıdır", MinimumLength = 2)]
        public String ExternalUserName { get; set; }
        public String ExternalEmail { get; set; }
        public String Ad { get; set; }
        public String Soyad { get; set; }
        public String ExternalLoginType { get; set; }
    }
    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }
    }
    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }
    public class ParolaDegistir{
        [Required]
        public String Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Lütfen parolanızı tekrar giriniz")]
        [Compare("Password", ErrorMessage = "Parolalar eşleşmiyor")]
        public String ConfirmPassword { get; set; }
        public String OldPassword { get; set; }
    }
    public class RoleEditModel
    {
        public IdentityRole Role { get; set; }
        public IEnumerable<ApplicationUser> Members { get; set; }
        public IEnumerable<ApplicationUser> NonMembers { get; set; }
    }
    public class RoleUpdateModel
    {
        public string RoleName { get; set; }
        public string RoleId { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
    public class Profil
    {
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string IsTanimi { get; set; }
        public string Ulke { get; set; }
        public DateTime? KatilmaTarihi { get; set; }
        public int SoruSayisi { get; set; }
        public int CevapSayisi { get; set; }
        public string Facebook { get; set; }
        public string BitBucket { get; set; }
        public string Twitter { get; set; }
        public string GitHub { get; set; }
        public string Hakkinda { get; set; }
    }
    public class TakipciModel
    {
        public int Id { get; set; }
        public string Takipci { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
    public class TakipEttikleriModel
    {
        public int Id { get; set; }
        public string TakipEttikleri { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
    public class FavoriSorularModel
    {
        public int Id { get; set; }
        public int FavoriSorular { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
    public class KullaniciKontrolModel
    {
        public ApplicationUser[] Kullanicilar { get; set; }
        public ApplicationUser[] Engelle { get; set; }
        public ApplicationUser[] EngelKaldir { get; set; }
        public IPagedList PagedList { get; set; }
    }
    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}