using BugHelper.Identity;
using BugHelper.Models;
using DotNetOpenAuth.GoogleOAuth2;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Membership.OpenAuth;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace BugHelper.Controllers
{
    [Authorize] // anonim erişimi engellemek için kullandığımız bir özellik 
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private Microsoft.AspNet.Identity.UserManager<ApplicationUser> userManager;
        private IdentityDataContext dc = new IdentityDataContext();
        private SorularContext sc = new SorularContext();
        private KullaniciKontrolModel kkm = new KullaniciKontrolModel();
        private SoruCevapModel sml = new SoruCevapModel();
        public const string psswrd = "AK5GrCpQxo7fbcn9N9saW6FrveU1xh65yzHcxxiooCIGQJsWrvH+YHnrbvjrLUr53Q==";
        public AccountController()
        {

            userManager = new Microsoft.AspNet.Identity.UserManager<ApplicationUser>(new UserStore<ApplicationUser>(dc)); // IdentityUser(SQL Serverimiz üzerinde çalışan sınıfımız)'dan türettiğimiz ApplicationUser sınıfımızın nesnelerinini taşıyan bir UserManager objesi ürettik(özet olarak bu obje, IdentityDataContext(Identity veri tabanımızın oluşumu) sınıfından bilgileri taşıyabiliyor)
            userManager.PasswordValidator = new CustomPasswordValidator(); // UserManager referansını kullanarak Identity yapısının default olarak tanımladığı parola doğrulayıcı sınıfını kullanmamak için custom bir PasswordValidator nesnesi oluşturup bunu kullanıyoruz)
            userManager.UserValidator = new Microsoft.AspNet.Identity.UserValidator<ApplicationUser>(userManager)
            {
                RequireUniqueEmail = true, //aynı e-mailden 1 tane olabilir
                AllowOnlyAlphanumericUserNames = false // alfanümerik(içinde rakam ve a-z arasında karakter bulunan) olmayan kullanıcı adlarını da kabul etmesi için false verdik
            };
        }
        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        public ActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ExternalLogin()
        {
            GoogleOAuth2Client.RewriteRequest();
            var redirectUrl = Url.Action("ExternalLogin");
            var authResult = OpenAuth.VerifyAuthentication(redirectUrl);
            string Email = authResult.ExtraData["email"];
            string Ad = authResult.ExtraData["given_name"];
            string Soyad = authResult.ExtraData["family_name"];

            if (dc.Users.Any(i => i.Email == Email))
            {
                LoginModel model = new LoginModel();
                model.UserName = dc.Users.Where(i => i.Email == Email).FirstOrDefault().UserName;
                model.Password = psswrd;
                return Login(model, "/Home/Index");
            }
            else
            {
                ExternalRegister model = new ExternalRegister();
                model.Ad = Ad;
                model.Soyad = Soyad;
                model.ExternalEmail = Email;
                model.ExternalLoginType = "google";
                TempData["ExternalMessage"] = "<div class=\"alert alert-info\" style=\"padding:10px;\">Google hesabınız ile bağlantı sağlandı</div>";
                return View("ExternalRegisterPage", model); ;
            }
        }
        [AllowAnonymous]
        public ActionResult ExternalLoginFB()
        {
            FacebookClient.RewriteRequest();
            var redirectUrl = Url.Action("ExternalLoginFB");
            var authResult = OpenAuth.VerifyAuthentication(redirectUrl);
            string Email = authResult.ExtraData["email"];
            string Ad = authResult.ExtraData["name"].Substring(0, authResult.ExtraData["name"].LastIndexOf(" "));
            string Soyad = authResult.ExtraData["name"].Substring(authResult.ExtraData["name"].LastIndexOf(" ") + 1);
            if (dc.Users.Any(i => i.Email == Email))
            {
                LoginModel model = new LoginModel();
                model.UserName = dc.Users.Where(i => i.Email == Email).FirstOrDefault().UserName;
                model.Password = psswrd;
                return Login(model, "/Home/Index");
            }
            else
            {
                ExternalRegister model = new ExternalRegister();
                model.Ad = Ad;
                model.Soyad = Soyad;
                model.ExternalEmail = Email;
                model.ExternalLoginType = "facebook";
                TempData["ExternalMessage"] = "<div class=\"alert alert-info\" style=\"padding:10px;\">Facebook hesabınız ile bağlantı sağlandı</div>";
                return View("ExternalRegisterPage", model); ;
            }
        }
        [AllowAnonymous]
        public ActionResult ExternalRegisterPage()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<ActionResult> ExternalRegisterPage(ExternalRegister model)
        {
            if (ModelState.IsValid)
            {
                if (!dc.Users.Any(i => i.UserName == model.ExternalUserName))
                {
                    var user = new ApplicationUser();
                    user.UserName = model.ExternalUserName;
                    user.Email = model.ExternalEmail;
                    user.KatilmaTarihi = System.DateTime.Now;
                    user.ExternalLoginType = model.ExternalLoginType;
                    var result = await userManager.CreateAsync(user, psswrd);
                    if (result.Succeeded)
                    {
                        dc.Users.Where(i => i.UserName == model.ExternalUserName).FirstOrDefault().Ad = model.Ad;
                        dc.Users.Where(i => i.UserName == model.ExternalUserName).FirstOrDefault().Soyad = model.Soyad;
                        dc.SaveChanges();
                        LoginModel login = new LoginModel();
                        login.UserName = model.ExternalUserName;
                        login.Password = psswrd;
                        return Login(login, "/Home/Index");
                    }
                    else
                    {
                        ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Kullanıcı adı mevcut</div>";
                        return View(model);
                    }
                }
            }
            return View(model);
        }
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost] //Metod, Post işlemi sağladığından eklendi. [HttpPost] önemli bilgileri geçirmek için kullanılır(örn.parola), ancak [HttpGet]'ten daha yavaştır.[HttpGet] daha hızlıdır ancak verileri urlye bağlar ve kullanıcının görebileceği halde sunar.Bu neden ile biz genelde Post işlemleri yapacağız
        [ValidateAntiForgeryToken] // Sql injection saldırılarına karşı platformun aldığı bir önlem, tokenler kullanılıyor
        [AllowAnonymous] // 12. satırda eklediğimiz özellik nedeniyle, kullanıcıların bu metoda ilişkin view'e ulaşabilmeleri için anonim kullanıma izin verdik
        public async Task<ActionResult> Register(Register model, string Email)
        {
            if (ModelState.IsValid) //ModelState.IsValid modelin durumunun geçerli olup olmadığını belirler. Model state modelin durumunu gösterir is valid diyerek bir nevi doğrulama yaparız. Örneğin kullanıcı custom olarak oluşturduğumuz parola oluşturma kurallarına uymaz ise ModelState.IsValid false döner ve bu hatayı foruma basar
            {

                var user = new ApplicationUser(); //yeni bir kullanıcı oluşturduk
                user.UserName = model.UserName; //kayıt formundan aldığımız kullanıcı adını oluşturduğumuz kullanıcıya atadık
                user.Email = Email; //aynı şekilde e-maili atadık
                user.KatilmaTarihi = System.DateTime.Now; //kullanıcı kayıt olurkenki zamanı System.DateTime.Now ile atadık(ctrl'e basılı tutarak now'a basarsanız sistemin date işlemlerine ilişkin diğer özelliklerini de görebilirsiniz)
                var result = await userManager.CreateAsync(user, model.Password); //userManager'ın Create tanımlamasına oluşturduğumuz kullanıcıyı ve formdan gelen şifreyi gönderdik, eğer oluşturma başarılı olursa Result.succeed true olacak ve kullanıcıyı oluşturacak
                if (result.Succeeded)
                {
                    return RedirectToAction("Login"); //oluşturma başarılı, login view'ine yönlendir
                }
                else
                {
                    foreach (var errors in result.Errors) //başarılı olmadı, kullanıcıyı oluştururken doğrulama amaçlı kullandığımız result referansının errors'una başarılı olamama nedeni yüklendi ve bunu Modelstate'in model errorları kısmına attık
                    {
                        ModelState.AddModelError("", errors); //hata ne ise modelstate'e ekle ki forma basabilsin
                    }
                }
            }
            ViewBag.HataMessage1 = "<div class=\"alert alert-danger\" role=\"alert\" style=\"overflow: hidden\">";
            ViewBag.HataMessage2 = "</div>";

            return View();
        }
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated) //kullanıcı girdiği sayfada gerekli role(yetki) sahip değilse
            {
                return View("Hata", new string[] { "Erişim hakkınız yok" });
            }

            ViewBag.returnUrl = returnUrl ?? "";
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            var user = UserManager.Find(model.UserName, model.Password);
                if(user != null)
            {
                if(user.LockoutEnabled == true)
                {
                    return View("Hata", "_Layout", "<div class=\"alert alert-warning\" role=\"alert\">Hesabınız banlandı</div>");
                }
            }
            if (ModelState.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true

                var result = SignInManager.PasswordSignIn(model.UserName, model.Password, isPersistent: true, shouldLockout: true);
                switch (result)
                {
                    case SignInStatus.Success:
                        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("VerifyCode", new { ReturnUrl = returnUrl });
                    case SignInStatus.Failure:
                    default:
                        ModelState.AddModelError("", "Yanlış kullanıcı adı veya şifre.");
                        return View(model);
                }
            }
            else
            {
                string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Hesabınız engellendi!</div>";
                return View("Hata", "_Layout", mesaj);
            }
        }


        [Authorize(Roles = "admin")]
        [HttpGet] //metod GET işlemi sağladığından eklendi
        public ActionResult BeklemeListesiSorular(int? page, string arananString) //beklemelistesine bekletilen soruları ve cevapları gönderiyoruz
        {
            if (arananString != null)
            {
                sml.PagedList = sc.Sorular.Where(i => i.SoruBaslik.Contains(arananString) ||
                i.SoruIcerik.Contains(arananString) || i.SorulmaTarihi.ToString().Contains(arananString) || i.SoruSahibi.Contains(arananString) || i.KodlamaDili.ToString().Contains(arananString) ||
                i.Id.ToString().Contains(arananString)).OrderBy(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                sml.SorularModelListe = sc.Sorular.Where(i => i.SoruBaslik.Contains(arananString) ||
                i.SoruIcerik.Contains(arananString) || i.SorulmaTarihi.ToString().Contains(arananString) || i.SoruSahibi.Contains(arananString) || i.KodlamaDili.ToString().Contains(arananString) ||
                i.Id.ToString().Contains(arananString)).OrderBy(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            else
            {
                sml.PagedList = sc.Sorular.Where(i => i.Onay == false).OrderBy(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                sml.SorularModelListe = sc.Sorular.Where(i => i.Onay == false).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            return View(sml);
        }
        [Authorize(Roles = "admin")]
        [HttpGet] //metod GET işlemi sağladığından eklendi
        public ActionResult BeklemeListesiCevaplar(int? page, string arananString) //beklemelistesine bekletilen soruları ve cevapları gönderiyoruz
        {
            if (arananString != null)
            {
                sml.PagedList = sc.Cevaplar.Where(i => i.Cevap.Contains(arananString) ||
                i.CevapTarihi.ToString().Contains(arananString) || i.CevapSahibi.ToString().Contains(arananString) || i.Id.ToString().Contains(arananString)
                ).OrderBy(i => i.CevapTarihi).OrderBy(i => i.CevapTarihi).ToPagedList(page ?? 1, 10);
                sml.CevaplarModelListe = sc.Cevaplar.Where(i => i.Cevap.Contains(arananString) ||
                i.CevapTarihi.ToString().Contains(arananString) || i.CevapSahibi.ToString().Contains(arananString) || i.Id.ToString().Contains(arananString)
                ).OrderBy(i => i.CevapTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            else
            {
                sml.PagedList = sc.Cevaplar.Where(i => i.Onay == false).OrderBy(i => i.CevapTarihi).ToPagedList(page ?? 1, 10);
                sml.CevaplarModelListe = sc.Cevaplar.Where(i => i.Onay == false).OrderByDescending(i => i.CevapTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            return View(sml);
        }
        [HttpPost]
        public ActionResult BeklemeListesiAction(SoruCevapModel model) //bekleme listesinden seçtiğimiz sorulara ya da cevaplara ilişkin işlemleri gerçekleştirdiğimiz metodumuz
        {
            if (model.SorularModelListe != null)
            {
                for (int i = 0; i < model.SorularModelListe.Count(); i++)
                {
                    foreach (var item in sc.Sorular.ToList())
                    {
                        if (item.Id == model.SorularModelListe.ElementAt(i).Id && model.SorularModelListe.ElementAt(i).Onay)
                        {
                            item.Onay = true;
                            sc.SaveChanges();
                        }
                    }
                }
                if (model.SorularModelListe.All(i => i.Onay == false))
                {
                    string mesaj = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Seçim yapılmadı</div>";
                    return PartialView("_Mesaj", mesaj);
                }
                else
                {
                    string mesaj = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Seçilen sorular başarı ile onaylandı</div>";
                    return PartialView("_Mesaj", mesaj);
                }
            }
            if (model.CevaplarModelListe != null)
            {
                for (int i = 0; i < model.CevaplarModelListe.Count(); i++)
                {
                    foreach (var item in sc.Cevaplar.ToList())
                    {
                        if (item.Id == model.CevaplarModelListe[i].Id && model.CevaplarModelListe[i].Onay)
                        {
                            item.Onay = true;
                            sc.SaveChanges();
                        }
                    }
                }
                if (model.CevaplarModelListe.All(i => i.Onay == false))
                {
                    string mesaj = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Seçim yapılmadı</div>";
                    return PartialView("_Mesaj", mesaj);
                }
                else
                {
                    string mesaj = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Seçilen cevaplar başarı ile onaylandı</div>";
                    return PartialView("_Mesaj", mesaj);
                }
            }
            return View();
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult KullaniciKontrolSil(int? page, string arananString)
        {
            if (arananString != null)
            {
                kkm.PagedList = dc.Users.Where(i => i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.Kullanicilar = dc.Users.Where(i => i.LockoutEnabled == false && i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            else
            {
                kkm.PagedList = dc.Users.Where(i => i.LockoutEnabled == false).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.Kullanicilar = dc.Users.Where(i => i.LockoutEnabled == false).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            for (int i = 0; i < kkm.Kullanicilar.Count(); i++)
            {
                kkm.Kullanicilar[i].Ad = kkm.Kullanicilar[i].Ad ?? "Belirtilmemiş";
                kkm.Kullanicilar[i].Soyad = kkm.Kullanicilar[i].Soyad ?? "Belirtilmemiş";
                kkm.Kullanicilar[i].Ulke = kkm.Kullanicilar[i].Ulke ?? "Belirtilmemiş";
            }
            return View(kkm);
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult KullaniciKontrolEngelle(int? page, string arananString)
        {
            if (arananString != null)
            {
                kkm.PagedList = dc.Users.Where(i => i.LockoutEnabled == false && i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.Engelle = dc.Users.Where(i => i.LockoutEnabled == false && i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            else
            {
                kkm.PagedList = dc.Users.Where(i => i.LockoutEnabled == false).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.Engelle = dc.Users.Where(i => i.LockoutEnabled == false).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            for (int i = 0; i < kkm.Engelle.Count(); i++)
            {
                kkm.Engelle[i].Ad = kkm.Engelle[i].Ad ?? "Belirtilmemiş";
                kkm.Engelle[i].Soyad = kkm.Engelle[i].Soyad ?? "Belirtilmemiş";
                kkm.Engelle[i].Ulke = kkm.Engelle[i].Ulke ?? "Belirtilmemiş";
            }
            return View(kkm);
        }
        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult KullaniciKontrolEngelKaldir(int? page, string arananString)
        {
            if (arananString != null)
            {
                kkm.PagedList = dc.Users.Where(i => i.LockoutEnabled == true && i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.EngelKaldir = dc.Users.Where(i => i.LockoutEnabled == true && i.UserName.Contains(arananString) ||
                i.Ad.Contains(arananString) || i.Soyad.Contains(arananString) || i.Email.Contains(arananString) || i.KatilmaTarihi.ToString().Contains(arananString) ||
                i.Ulke.Contains(arananString)).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            else
            {
                kkm.PagedList = dc.Users.Where(i => i.LockoutEnabled == true).OrderBy(i => i.KatilmaTarihi).ToPagedList(page ?? 1, 10);
                kkm.EngelKaldir = dc.Users.Where(i => i.LockoutEnabled == true).OrderByDescending(i => i.KatilmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToArray();
            }
            for (int i = 0; i < kkm.EngelKaldir.Count(); i++)
            {
                kkm.EngelKaldir[i].Ad = kkm.EngelKaldir[i].Ad ?? "Belirtilmemiş";
                kkm.EngelKaldir[i].Soyad = kkm.EngelKaldir[i].Soyad ?? "Belirtilmemiş";
                kkm.EngelKaldir[i].Ulke = kkm.EngelKaldir[i].Ulke ?? "Belirtilmemiş";
            }
            return View(kkm);
        }
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async System.Threading.Tasks.Task<ActionResult> KullaniciKontrolActionAsync(KullaniciKontrolModel users, string durum) //kullanıcı yönetimi için kullandığımız metod
        {
            if (users != null) //view'den metoda gelen users'in boş olması durumunda NullReferenceException hatası almamak için kontrol yapıyoruz, NullReferenceException hatası için bir çok durumda buna benzer kontroller yapacağız
            {
                if (durum.Equals("sil")) //durum silme durumu ise
                {
                    for (int i = 0; i < users.Kullanicilar.Count(); i++) //gelen users modelinin içindeki kullanıcılar dizisinin uzunluğu kadar döndürüyoruz
                    {
                        if (users.Kullanicilar[i].Kontrol == true) //view'de kullandığımız checkbox'ta işaretlenenlerin kontrol değeri true olduğundan, kontrol değeri true olan objeleri seçiyoruz
                        {
                            var user = userManager.FindById(users.Kullanicilar[i].Id); //seçtiğimiz userları, oluşturduğumuz yeni bir kullanıcı referansına yönlendirip daha sonra 145. satırda seçilen user'ı siliyoruz
                            dc.Takipci.RemoveRange(dc.Takipci.Where(a => a.ApplicationUser.Id == user.Id));
                            dc.TakipEttikleri.RemoveRange(dc.TakipEttikleri.Where(a => a.ApplicationUser.Id == user.Id));
                            dc.FavoriSorular.RemoveRange(dc.FavoriSorular.Where(a => a.ApplicationUser.Id == user.Id));
                            userManager.Delete(user);
                        }
                    }
                    if (users.Kullanicilar.All(i => i.Kontrol == false))
                    {
                        string mesaj = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Seçim yapılmadı</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                    else
                    {
                        string mesaj = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Seçilen kullanıcılar başarı ile silindi</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                }
                else if (durum.Equals("engelle")) //engelleme durumu için
                {
                    for (int i = 0; i < users.Engelle.Count(); i++)
                    {
                        if (users.Engelle[i].Kontrol)
                        {
                            await userManager.SetLockoutEnabledAsync(users.Engelle[i].Id, true);//yukarıdaki durumla silme durumu ile aynı, bu sayırda kullanıcının default olarak false olan LockoutEnabled değerini true yapıyoruz ve kullanıcı engelleniyor.Daha sonra tarih vermediğimiz için sınırsız banlanmış oluyor
                            await userManager.UpdateAsync(userManager.FindById(users.Engelle[i].Id));//banlanan user'ın durumunu güncelliyoruz
                        } //işlemlerimizin başında await kullanmamızın nedeni bu işlemleri asenkron olarak yapmak istememiz, aksi durumda hata ile karşılaşıyoruz
                    }
                    if (users.Engelle.All(i => i.Kontrol == false))
                    {
                        string mesaj = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Seçim yapılmadı</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                    else
                    {
                        string mesaj = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Seçilen kullanıcılar başarı ile engellendi</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                }
                else if (durum.Equals("engelkaldir")) //engel kaldırma durumu için
                {
                    for (int i = 0; i < users.EngelKaldir.Count(); i++)
                    {
                        if (users.EngelKaldir[i].Kontrol)
                        {
                            await userManager.SetLockoutEnabledAsync(users.EngelKaldir[i].Id, false);
                            await userManager.UpdateAsync(userManager.FindById(users.EngelKaldir[i].Id));
                        }
                    }
                    if (users.EngelKaldir.All(i => i.Kontrol == false))
                    {
                        string mesaj = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Seçim yapılmadı</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                    else
                    {
                        string mesaj = "<div class=\"alert alert-success\" role=\"alert\" style=\"overflow: hidden\">✔Seçilen kullanıcıların engeli başarı ile kaldırıldı</div>";
                        return PartialView("_Mesaj", mesaj);
                    }
                }
            }
            return View();
        }
        private ApplicationUser Kontrol(ApplicationUser identity)
        {
            return identity;
        }
        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut(); //kullanıcıyı çıkış yaptırıp kimlik doğrulamasını sona erdiriyoruz

            return RedirectToAction("Login"); //login sayfasına yönlendiriyoruz
        }
        [AllowAnonymous]
        public ActionResult RedirectToExternalLogin()
        {
            string provider = "google";
            string returnUrl = "";
            return new ExternalLoginResult(provider, Url.Action("ExternalLogin", new { ReturnUrl = returnUrl }));
        }
        [AllowAnonymous]
        public ActionResult RedirectToExternalLoginFB()
        {
            string provider = "facebook";
            string returnUrl = "";
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginFB", new { ReturnUrl = returnUrl }));
        }
        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OpenAuth.RequestAuthentication(Provider, ReturnUrl);
            }
        }
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HataMessage1 = "<div class=\"alert alert-danger\" role=\"alert\" style=\"overflow: hidden\">";
                ViewBag.HataMessage2 = "</div>";
                return View(model);
            }
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Kullanıcı bulunamadı!</div>";
                return View("Hata", mesaj);
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            else
            {
                foreach (var errors in result.Errors)
                {
                    ModelState.AddModelError("", errors);
                }
            }
            return View();
        }
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            var user = UserManager.FindByEmail(email);
            if (user == null)
            {
                string mesaj = "<div class=\"alert alert-warning\" role=\"alert\">Kullanıcı bulunamadı!</div>";
                return View("Hata", "_Layout", mesaj);
            }
            string code = UserManager.GeneratePasswordResetToken(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account",
        new { UserId = user.Id, code = code }, protocol: Request.Url.Scheme);
            await UserManager.SendEmailAsync(user.Id, "Parola Sıfırlama", "Parolanı  " + callbackUrl + " linkine tıklayarak sıfırla");
            return View("ForgotPasswordConfirmation");
        }
        public ActionResult UyelikIptal()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UyelikIptal(string parola)
        {
                var user = UserManager.FindByName(User.Identity.Name);
                if (UserManager.CheckPassword(user, parola))
                {
                    dc.Takipci.RemoveRange(dc.Takipci.Where(i => i.ApplicationUser.Id == user.Id));
                    dc.TakipEttikleri.RemoveRange(dc.TakipEttikleri.Where(i => i.ApplicationUser.Id == user.Id));
                    dc.FavoriSorular.RemoveRange(dc.FavoriSorular.Where(i => i.ApplicationUser.Id == user.Id));
                    dc.SaveChanges();
                    var authManager = HttpContext.GetOwinContext().Authentication;
                    authManager.SignOut();
                    UserManager.Delete(user);
                    return JavaScript("yonlendir()");
                }
                else
                {
                    string msg = "<div class=\"alert alert-warning\" role=\"alert\" style=\"overflow: hidden\">Parola yanlış</div>";
                    return PartialView("_Mesaj", msg);
                }
        }
        [AllowAnonymous]
        public ActionResult TwoFAEnable()
        {
            string durum;
            if (dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().TwoFactorEnabled)
            {
                durum = "Aktif";
            }
            else
            {
                durum = "Pasif";
            }
            return View("TwoFAEnable", "_Layout", durum);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult TwoFAEnable(string durum)
        {
            var user = UserManager.FindByName(User.Identity.Name);
            if (durum.Equals("Aktif"))
            {
                UserManager.SetTwoFactorEnabled(user.Id, true);
                user.EmailConfirmed = true;
            }
            else
            {
                if (durum.Equals("Pasif"))
                {
                    UserManager.SetTwoFactorEnabled(user.Id, false);
                    user.EmailConfirmed = true;
                }
            }
            UserManager.Update(UserManager.FindByName(User.Identity.Name));
            return View("TwoFAEnable", "_Layout", durum);
        }
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string returnUrl)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var result = await SignInManager.SendTwoFactorCodeAsync("Email Code");
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = "Email Code", ReturnUrl = returnUrl});
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: false, false);
            switch (result)
            {
                case SignInStatus.Success:
                    return Redirect(Url.Action("Index","Home"));
                case SignInStatus.LockedOut:
                    return View("Hata", "_Layout", "<div class=\"alert alert-warning\" role=\"alert\">Fazla hatalı giriş yaptığınız için hesabınız <b>5<b> dakika kilitlendi</div>");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Kod yanlış.");
                    return View(model);
            }
        }
        public ActionResult IletısımMesajlar(int? page)
        {
            IletisimPagedModel ipm = new IletisimPagedModel();
            ipm.PagedList = dc.Iletisim.Select(i => i).OrderBy(i => i.Id).ToPagedList(page ?? 1, 10);
            ipm.IletisimModelList = dc.Iletisim.Select(i => i).OrderBy(i => i.Id).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
            return View(ipm);
        }
    }
}
