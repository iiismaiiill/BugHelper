using BugHelper.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugHelper.Models
{
    public class ProfilModel
    {
        public List<SorularModel> KullaniciSorulari { get; set; }
        public List<SorularModel> KullaniciCevapSorulari { get; set; }
        public List<CevaplarModel> KullaniciCevaplari { get; set; }
        public List<SorularModel> FavoriListesi { get; set; }
        public List<TakipciModel> Takipciler { get; set; }
        public List<TakipEttikleriModel> TakipEdilenler { get; set; }
        public int SoruSayisi { get; set; }
        public int CevapSayisi { get; set; }
        public ApplicationUser Kullanici { get; set; }
        public bool TakipteMi = false;
    }
    public class EmailVal
    {
        [Required(ErrorMessage = "Parola boş bırakılamaz")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Email boş bırakılamaz")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                           @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                           ErrorMessage = "Email adresi geçersiz")]
        public string NewEmail { get; set; }

        [Required(ErrorMessage = "Email boş bırakılamaz")]
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                           @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                           @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
                           ErrorMessage = "Email adresi geçersiz")]
        public string ConfirmEmail { get; set; }
    }
    public class FotoModel
    {
        public HttpPostedFileBase FotoFile { get; set; }
        public string Path { get; set; }
    }
}