using BugHelper.Identity;
using BugHelper.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace BugHelper.Controllers
{
    [Authorize]
    public class KullaniciController : Controller
    {
        private SorularContext sc = new SorularContext();
        private IdentityDataContext dc = new IdentityDataContext();
        private ProfilModel model = new ProfilModel();
        private Profil profilGuncelle = new Profil();
        private Register register = new Register();
        private SoruCevapModel scm = new SoruCevapModel();
        private UserManager<ApplicationUser> userManager;
        public KullaniciController()
        {
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityDataContext()));
            userManager.PasswordValidator = new CustomPasswordValidator();
        }
        [Route("kullanici/{kullaniciAdi}/favoriler")]//Route, bu metod çalıştığında metodu çağıran görevin gönderdiği url değerlerini alır({} içindeki kısım, metodu çağıran görevden gelir, diğer kısımlar sabittir(bkz. ~/Views/Sorular/Soru.cshtml satır 75))
        public ActionResult FavoriSorular()
        {
            ProfilModel model = new ProfilModel(); //Favori soruları göstermek için kullandığımız view'e göndereceğimiz model için bir referans ve obje oluşturduk(Ctrl'e basılı tutarak ProfilModel'e tıklarsanız model için kullandığımız sınıfı görebilirsiniz)
            model.FavoriListesi = new List<SorularModel>(); //ProfilModel sınıfından türettiğimiz objenin FavoriListesi(SorularModel nesnelerinden oluşan bir List(liste)) değişkeninine bir obje(new List<SorularModel>()) atadık çünkü, eğer bu listeyi kullanmadan önce tanımlamaz isek metoddaki kodlar okunurken bu listeyi kullandığımız yerde listeyi null olarak tanımlayıp kullanamayacak ve bize NullReferenceException hatası dönecektir
            if (dc.FavoriSorular.Where(i => i.ApplicationUser.UserName == User.Identity.Name).FirstOrDefault() != null) //dc referansını identity contexte(identity databasei için) ulaşmak için kullanırız(bkz. satır 17).dc.FavorSorular DbSet'inden(FavoriSorularModel nesnelerinden oluşan DbSet(databasedeki tablolar)), ApplicationUser.UserName'i giriş yapan kullanıcının UserName'ine eşit olan FavoriSorular nesnelerinin ilkini FirstOrDefault()(bulduğu ilk nesneyi alır) ile alıyoruz ve null olup olmadığınız kontrol ediyoruz(bkz. NullReferenceException)
            { //User.Identity.Name, giriş yapan kullanıcının kullanıcı adını verir
                foreach (var item in dc.FavoriSorular.Where(i => i.ApplicationUser.UserName == User.Identity.Name).ToList())//bu linq sorgusu da diğerleri ile benzer bu sorgu ile bulduğumuz listenin içinde foreach ile dönüyoruz
                {
                    foreach (var soru in sc.Sorular.ToList())//bu döngüdeki amaç favori soruları göstermek üzere kullanacağımız view için kullandığımız ProfilModel modelinin içindeki FavoriListesine(bkz. model.FavoriListesi), linq sorgusu ile bulduğumuz listenin içindeki FavoriSorular(bu bir int değer ve soru idsini temsil ediyor(bkz. Models klasöründeki UserModel içindeki FavoriSorularModel)) değerleri ile sc.Sorular(soruların database'i) üzerinde döndüğümüz soru.Id değerleri eşit olanları atmak
                    {
                        if (soru.Id == item.FavoriSorular)
                        {
                            model.FavoriListesi.Add(soru);
                        }
                    }
                }
                return View(model);
            }
            return View();
        }
        [AllowAnonymous]
        [Route("kullanici/{takmaAd}/{KullaniciAdi}")]
        public ActionResult ProfilGoruntule()
        {
            string kullaniciAdi = (RouteData.Values["KullaniciAdi"].ToString());//urlye gelen KullaniciAdi değerini çekip oluşturduğumuz stringe daha sonra kullanmak üzere atıyoruz
            string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Kullanıcı bulunamadı</div>";
            model.SoruSayisi = sc.Sorular.Count(i => i.SoruSahibi == kullaniciAdi);
            model.CevapSayisi = sc.Cevaplar.Where(i => i.CevapSahibi == kullaniciAdi).Count();
            model.Kullanici = dc.Users.Where(i => i.UserName == kullaniciAdi).FirstOrDefault();
            if (model.Kullanici == null) return View("Hata", "_Layout", mesaj);
            model.KullaniciSorulari = sc.Sorular.Where(i => i.SoruSahibi == kullaniciAdi).OrderByDescending(i => i.Id).Take(5).ToList();//sorular database'inden, profiline girdiğimiz kullanıcı adı ile ilişkili soruları Id değerine göre azalan şekilde sırala(burada amaç listeyi ters çevirip son 5 soruyu almak) ve 5'ini al
            model.KullaniciCevaplari = sc.Cevaplar.Where(i => i.CevapSahibi == kullaniciAdi).OrderByDescending(i => i.Id).Take(5).ToList();
            model.KullaniciCevapSorulari = new List<SorularModel>(); //(NullReferenceException)
            if (dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi).FirstOrDefault() != null)//(NullReferenceException)
            {
                model.Takipciler = dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi).ToList();
            }
            if (User.Identity.IsAuthenticated)
            {
                if (dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == kullaniciAdi).FirstOrDefault() != null)//(NullReferenceException)
                {
                    model.TakipEdilenler = dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == kullaniciAdi).ToList();//linq sorgularının mantığı aynı olduğu için bunlara açıklama yapmıyorum
                }
            }
            for (int x = 0; x < model.KullaniciCevaplari.Count; x++)
            {
                var temp = model.KullaniciCevaplari.ElementAt(x).Id;
                model.KullaniciCevapSorulari.Add(sc.Cevaplar.Where(i => i.Id == temp).Select(a => a.Soru).FirstOrDefault() ?? new SorularModel { SoruBaslik = "Cevap bulunamadı" });
            }
            if (User.Identity.IsAuthenticated && dc.TakipEttikleri.Any(i => i.ApplicationUser.UserName == User.Identity.Name && i.TakipEttikleri == kullaniciAdi))//(NullReferenceException) için farklı bir kullanım.Sorgu yaparken Any kullanırsak sorgu yaptığımız yerde parantez içine aldığımız nesneden olup olmadığını boolean değer olarak söyler, bu şekilde de NullReferenceException handling yapabiliriz
            {                                   //bu kontroldeki amaç giriş yapan kullanıcının profiline girdiği kullanıcıyı takıp edip etmediğine dair bir boolean değer kullanmak ve bu değer ile view'de takip butonunu gerektiği şekilde göstermek(takip et ya da takipten çık)
                model.TakipteMi = true;
            }
            return View(model);
        }
        [Route("{kullaniciAdi}/profil")]
        public ActionResult ProfiliGuncelle() //ProfiliGuncelle() ve ProfiliGuncelleAction() adlı iki metod kullanmamın sebebi birinin route değerleri alması
        {
            Profil profil = new Profil();
            var user = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            profil.Ad = user.Ad;
            profil.Soyad = user.Soyad;
            profil.IsTanimi = user.IsTanimi;
            profil.Ulke = user.Ulke;
            profil.Hakkinda = user.Hakkinda;
            profil.Facebook = user.Facebook;
            profil.Twitter = user.Twitter;
            profil.GitHub = user.GitHub;
            profil.BitBucket = user.BitBucket;
            IEnumerable<String> ulkeler = dc.Ulkeler.Select(i => i.Ulkeler).ToList();
            ViewBag.Ulkeler = ulkeler;
            return View(profil);
        }

        [HttpPost]
        public ActionResult ProfiliGuncelleAction(Profil gelenProfil)
        {
            string mesaj = "<div class=\"alert alert-success\" role=\"alert\">Profil güncelleme başarılı...</div>";
            var user = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            user.Ad = gelenProfil.Ad; //gelen form verilerine göre bu verilerin veritabanına aktarımı
            user.Soyad = gelenProfil.Soyad;
            user.Ulke = gelenProfil.Ulke;
            user.IsTanimi = gelenProfil.IsTanimi;
            user.Hakkinda = gelenProfil.Hakkinda;
            user.Facebook = gelenProfil.Facebook;
            user.Twitter = gelenProfil.Twitter;
            user.GitHub = gelenProfil.GitHub;
            user.BitBucket = gelenProfil.BitBucket;
            dc.SaveChanges();
            return PartialView("_Mesaj", mesaj);
        }


        [Route("{kullaniciAdi}/paroladegistir")]
        [HttpGet]
        public ActionResult ParolaDegistir()
        {
            if (dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().ExternalLoginType == null)
            {
                return View();
            }
            else
            {
                ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Dış kayıtlı kullanıcılar parola değiştiremez</div>";
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("{kullaniciAdi}/paroladegistir")]
        public ActionResult ParolaDegistir(ParolaDegistir gelenParola)
        {
            if (ModelState.IsValid)
            {
                var result = userManager.ChangePassword(User.Identity.GetUserId(), gelenParola.OldPassword, gelenParola.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Parolanız başarı ile değiştirildi</div>";
                    return View();
                }
                else
                {
                    foreach (var errors in result.Errors)
                    {
                        ModelState.AddModelError("", "Parola yanlış");
                    }
                }
            }
            ViewBag.HataMessage1 = "<div class=\"alert alert-danger\" role=\"alert\" style=\"overflow: hidden\">";
            ViewBag.HataMessage2 = "</div>";
            return View("ParolaDegistir");
        }
        [HttpPost]
        public PartialViewResult TakipEt(string kullaniciAdi)
        {
            dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().TakipEttikleri.Add(new TakipEttikleriModel //Burada Foreign Key'in kullanımı vardır. Görüldüğü üzere Users üzerinden TakipEttikleri ekleniyor ve bu ikisi Entity Framework tarafından foreign key ile ilişkilendiriliyor. Daha sonra bunlara ilişkin değerlere birbirleri üzerinden erişebiliyoruz 
            {                                       //alttaki kod satırları da benzer şekildeki linq sorgularından ibarettir
                TakipEttikleri = kullaniciAdi
            });
            dc.Users.Where(i => i.UserName == kullaniciAdi).FirstOrDefault().Takipci.Add(new TakipciModel
            {
                Takipci = User.Identity.Name
            });
            dc.SaveChanges();
            string mesaj = "<div class=\"alert alert-success\" role=\"alert\">" + kullaniciAdi + " adlı kullanıcıyı takip ediyorsunuz</div>";
            return PartialView("_Mesaj", mesaj);
        }
        public PartialViewResult TakiptenCik(string kullaniciAdi)
        {
            dc.TakipEttikleri.Remove(dc.TakipEttikleri.Where(i => i.ApplicationUser.UserName == User.Identity.Name && i.TakipEttikleri == kullaniciAdi).FirstOrDefault());
            dc.Takipci.Remove(dc.Takipci.Where(i => i.ApplicationUser.UserName == kullaniciAdi && i.Takipci == User.Identity.Name).FirstOrDefault());
            dc.SaveChanges();
            string mesaj = "<div class=\"alert alert-success\" role=\"alert\">" + kullaniciAdi + " adlı kullanıcıyı takipten çıktınız</div>";
            return PartialView("_Mesaj", mesaj);
        }

        [HttpPost]
        public ActionResult Favori(int soruId)
        {
            if (dc.FavoriSorular.Where(i => i.FavoriSorular == soruId && i.ApplicationUser.UserName == User.Identity.Name).FirstOrDefault() != null)
            {
                scm.FavorideMi = false;
                scm.SoruId = soruId;
                dc.FavoriSorular.Remove(dc.FavoriSorular.Where(i => i.ApplicationUser.UserName == User.Identity.Name && i.FavoriSorular == soruId).FirstOrDefault());
                dc.SaveChanges();
                return PartialView(scm);
            }
            else
            {
                scm.FavorideMi = true;
                scm.SoruId = soruId;
                dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().FavoriSorular.Add(new FavoriSorularModel
                {
                    FavoriSorular = soruId
                });
                dc.SaveChanges();
                return PartialView(scm);
            }
        }
        [HttpPost]
        public ActionResult FavoridenCikar(int soruId)
        {
            dc.FavoriSorular.Remove(dc.FavoriSorular.Where(i => i.ApplicationUser.UserName == User.Identity.Name && i.FavoriSorular == soruId).FirstOrDefault());
            dc.SaveChanges();
            return PartialView("_Mesaj");
        }
        [Route("{kullaniciAdi}/profil/fotoguncelle")]
        public ActionResult FotoGuncelle()
        {
            FotoModel fm = new FotoModel();
            fm.Path = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().Path;
            return View(fm);
        }
        [HttpPost]
        [Route("{kullaniciAdi}/profil/fotoguncelle")]
        public ActionResult FotoGuncelle(FotoModel file)
        {
            Random rnd = new Random();
            string mesaj;
            decimal DosyaBoyutu = 500;
            try
            {

                var desteklenenTip = new[] { "jpg", "jpeg", "png" };
                if (file != null) {
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Herhangi bir dosya seçmediniz.</div>";
                    return View(file);
                }
                var fileExt = System.IO.Path.GetExtension(file.FotoFile.FileName).Substring(1);
                if (!desteklenenTip.Contains(fileExt))
                {
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Geçersiz dosya formatı. Yükleyeceğiniz fotoğraf jpg veya png olmalıdır.</div>";
                    return View();
                }
                else if (file == null || file.FotoFile.ContentLength > (DosyaBoyutu * 1024))
                {
                    mesaj = "Dosya boyutu en fazla " + DosyaBoyutu + "KB olmalıdır!";
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">" + mesaj + "</div>";

                    return View();
                }
                else
                {
                    FotoModel fm = new FotoModel();
                    string key = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    char[] stringChars = new char[7];
                    var _FileName = Path.GetFileName(file.FotoFile.FileName);
                    while (true)
                    {
                        for (int i = 0; i < stringChars.Length; i++)
                        {
                            stringChars[i] = key[rnd.Next(key.Length)];
                        }
                        key = new string(stringChars);
                        if (dc.Users.All(i => i.PhotoKey != key))
                        {
                            break;
                        }
                    }
                    var _path = Path.Combine(Server.MapPath("~/img"), key + _FileName);
                    file.FotoFile.SaveAs(_path);
                    var user = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
                    user.FileName = _FileName;
                    user.Path = _path;
                    user.PhotoKey = key;
                    dc.SaveChanges();
                    fm.Path = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().Path;
                    ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Güncelleme Başarılı</div>";
                    return View(fm);
                }
            }
            catch
            {
                ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Hata Oluştu. Tekrar Deneyiniz.</div>";
                return View();
            }
        }
        [Route("{kullaniciAdi}/epostadegistir")]
        public ActionResult EpostaDegistir()
        {
            if (dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().ExternalLoginType == null)
            {
                return View();
            }
            else
            {
                string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Dış kayıtlı kullanıcılar e-posta değiştiremez</div>";
                return View("Hata", "_Layout", mesaj);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EpostaDegistirAction(EmailVal model)
        {
            ApplicationUser user = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            if (model.CurrentPassword == null || model.NewEmail == null || model.ConfirmEmail == null)
            {
                ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\"> E-Posta alanları boş bırakılamaz...</div>";

                return View("EpostaDegistir");
            }
            if (userManager.CheckPassword(user, model.CurrentPassword))
            {
                if (model.NewEmail == model.ConfirmEmail)
                {
                    if (dc.Users.All(i => i.Email != model.NewEmail))
                    {
                        user.Email = model.NewEmail;
                        dc.SaveChanges();
                        ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">E-Posta güncelleme başarılı...</div>";
                        return View("EpostaDegistir");
                    }
                    else
                    {
                        ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">E-Posta zaten kayıtlı</div>";
                        return View("EpostaDegistir");
                    }
                }
                else
                {
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">E-Postalar uyuşmuyor...</div>";
                    return View("EpostaDegistir");
                }
            }
            else
            {
                ViewBag.Message = "<div class=\"alert alert-danger\" role=\"alert\">Parola hatalı...</div>";
                return View();
            }
        }
        [AllowAnonymous]
        public ActionResult Iletisim()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Iletisim(IletisimModel model)
        {
            ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Mesajınız bize başarı ile gönderildi...</div>";
            dc.Iletisim.Add(model);
            dc.SaveChanges();
            return View();
        }
    }
}