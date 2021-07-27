using BugHelper.Models;
using System.Linq;
using System.Web.Mvc;
using BugHelper.Scripts.SoruUrl;
using PagedList;
using BugHelper.Identity;

namespace BugHelper.Controllers
{
    public class CevaplarController : Controller
    {
        private SorularContext sc = new SorularContext();
        private CevaplarModel cm = new CevaplarModel();
        private IdentityDataContext dc = new IdentityDataContext();

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public PartialViewResult Cevapla(int soruIdCevap, string gelenCevap,int? misafir) //soruların cevaplandığında kullanacağımız metod
        {
            sc.Sorular.Where(i => i.Id == soruIdCevap).FirstOrDefault().CevapSayisi++;
            if (User.Identity.IsAuthenticated && misafir != 1) //eğer kullanıcı giriş yapmış ise
            {
                var cevap = new CevaplarModel //gelen cevap için bir CevapModel objesi oluşturduk ve gelen cevabın değerlerini bu objeye attık
                {
                    Cevap = gelenCevap,
                    CevapSahibi = User.Identity.Name,
                    CevapTarihi = System.DateTime.Now,
                    Onay = true //kullanıcı giriş yaptığı için onay durumunu true yapıyoruz
                };
                sc.Sorular.Where(i => i.Id == soruIdCevap).FirstOrDefault().Cevaplar.Add(cevap); //soruIdCevap, view'den gelen bir değerdir ve kullanıcının cevabı yazdığı sorunun ID'sidir.Bu soruyu veritabanından bulup, soru üzerinden Cevaplar'a bu cevabı ekliyoruz.Entity framework, foreign key(tabloları birbiri ile ilişkilendirmek için kullanılan bir anahtar diyebiliriz) ile soru ile cevabı bağlıyor ve daha sonradan bu değişkenlere birbirleri üzerinden erişebiliyoruz
                sc.SaveChanges(); //database'de yaptığımız değişiklikleri kaydediyoruz
            }
            else
            {
                var cevap = new CevaplarModel //kullanıcı giriş yapmamışsa onay durumunu false(giriş yapmadığı için), cevabın sahibini de misafir olarak belirliyoruz
                {
                    Cevap = gelenCevap,
                    CevapSahibi = "Misafir",
                    CevapTarihi = System.DateTime.Now,
                    Onay = false
                };
                sc.Sorular.Where(i => i.Id == soruIdCevap).FirstOrDefault().Cevaplar.Add(cevap);
                sc.SaveChanges();
            }
            if (User.Identity.IsAuthenticated && misafir != 1) //eğer giriş yapmışsa kullanıcının girdiği cevabı soruya ekle
            {
                try
                {
                    string mesaj = "<div class=\"alert alert-success\" role=\"alert\">Cevabınız başarı ile eklendi</div>";
                    return PartialView("_Mesaj", mesaj);
                }
                catch
                {
                    string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Hata oluştu</div>";
                    return PartialView("_Mesaj", mesaj);
                }
            }
            else//giriş yapmamışsa mesajı bastırıp cevabı bir yönetici onaylayana kadar bekleme listesine alıyoruz
            {
                
                string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Misafir kullanıcı olduğunuz için cevabınız onaylanana dek kullanıcılara kapalı olacaktır.</div>";
                return PartialView("_Mesaj", mesaj);
            }
        }
        public PartialViewResult CevaplariGoster() {
            return PartialView();
        }
        [HttpGet]
        public ActionResult CevapDuzenle(int cevapId)
        {
            CevaplarModel cevap = sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault();
            var soru = sc.Sorular.Where(i => i.Cevaplar.Where(a => a.Id == cevapId).FirstOrDefault() != null).FirstOrDefault();
            cevap.Soru = soru;
            return View(cevap);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CevapDuzenle(CevaplarModel degistirilecekCevap)
        {
            var eskiCevap = sc.Cevaplar.Where(i => i.Id == degistirilecekCevap.Id).FirstOrDefault();
            eskiCevap.Cevap = degistirilecekCevap.Cevap;
            sc.SaveChanges();
            TempData["Duzenlendi"] = "<div class=\"alert alert-success\" role=\"alert\">Cevabınız başarı ile düzenlendi</div>";
            return Redirect(Url.Action("Soru", "Sorular", new { baslik = FriendlyURL.FriendlyURLTitle(degistirilecekCevap.Soru.SoruBaslik), SoruId = degistirilecekCevap.Soru.Id }));
        }
        [HttpPost]
        public ActionResult CevapSil(CevaplarModel silinecekCevap)
        {
            sc.ArtiOyCevaplar.RemoveRange(sc.ArtiOyCevaplar.Where(i => i.Cevap.Id == silinecekCevap.Id));
            sc.EksiOyCevaplar.RemoveRange(sc.EksiOyCevaplar.Where(i => i.Cevap.Id == silinecekCevap.Id));
            sc.Sorular.Where(i => i.Cevaplar.Where(a => a.Id == silinecekCevap.Id).FirstOrDefault() != null).FirstOrDefault().Cevaplar.Remove(sc.Cevaplar.Where(b => b.Id == silinecekCevap.Id).FirstOrDefault());
            sc.Cevaplar.Remove(sc.Cevaplar.Where(i => i.Id == silinecekCevap.Id).FirstOrDefault());
            sc.SaveChanges();
            TempData["Duzenlendi"] = "<div class=\"alert alert-success\" role=\"alert\">Cevabınız başarı ile silindi</div>";
            return Redirect(Url.Action("Soru", "Sorular", new { baslik = FriendlyURL.FriendlyURLTitle(silinecekCevap.Soru.SoruBaslik), SoruId = silinecekCevap.Soru.Id }));
        }
        [HttpPost]
        public ActionResult CevaplarFiltre(string etiketFiltre, int soruId, int? page)
        {
            SoruCevapModel scm = new SoruCevapModel();
            scm.SoruCevapContext = sc;
            scm.CevaplarModelForSoru = sc.Cevaplar.Where(i => i.Soru.Id == soruId && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
            foreach (var item in scm.CevaplarModelForSoru)
            {
                if (item.CevapSahibi == "Misafir") { continue; }
                item.CevapSahibiPath = dc.Users.Where(i => i.UserName == item.CevapSahibi).FirstOrDefault().Path;
            }
            scm.Soru = sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault();
            if (etiketFiltre.Equals("Yeniler")) {
                scm.PagedList = sc.Cevaplar.Where(i => i.Soru.Id == soruId && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).ToPagedList(page ?? 1, 10);
                scm.CevaplarModelForSoru = sc.Cevaplar.Where(i => i.Soru.Id == soruId && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                return PartialView(scm);
            }
            else
            {
                scm.PagedList = sc.Cevaplar.Where(i => i.Soru.Id == soruId && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).ToPagedList(page ?? 1, 10);
                scm.CevaplarModelForSoru = sc.Cevaplar.Where(i => i.Soru.Id == soruId && i.Onay == true && i.Soru.Onay == true).OrderByDescending(i => i.Deger).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                return PartialView(scm);
            }
        }
    }
}