using BugHelper.Identity;
using BugHelper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BugHelper.Scripts.SoruUrl;
using PagedList;

namespace BugHelper.Controllers
{
    public class SorularController : Controller
    {
        private IdentityDataContext dc = new IdentityDataContext();
        private List<SorularModel> bekletilenListesi = new List<SorularModel>();
        private SorularContext sc = new SorularContext();
        [HttpGet]
        public ActionResult SoruSor()
        {
            IEnumerable<String> diller = sc.Etiketler.Select(i => i.KodlamaDili);
            ViewBag.Diller = diller;
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SoruSor(SorularModel soruSor) //soru sorma view'imiz için kullandığımız metoddur
        {

            if (User.Identity.IsAuthenticated)
            {
                var soru = new SorularModel
                {
                    SoruSahibi = User.Identity.Name,
                    KodlamaDili = soruSor.KodlamaDili,
                    SoruBaslik = soruSor.SoruBaslik,
                    SoruIcerik = soruSor.SoruIcerik,
                    SorulmaTarihi = System.DateTime.Now,
                    Onay = true
                };
                sc.Sorular.Add(soru);
                sc.SaveChanges();
                TempData["Message"] = "<div class=\"alert alert-success\" role=\"alert\">Sorunuz başarı ile eklendi</div>";
            }
            else
            {
                var soru = new SorularModel
                {
                    SoruSahibi = "Misafir",
                    KodlamaDili = soruSor.KodlamaDili,
                    SoruBaslik = soruSor.SoruBaslik,
                    SoruIcerik = soruSor.SoruIcerik,
                    SorulmaTarihi = System.DateTime.Now,
                    Onay = false
                };
                sc.Sorular.Add(soru);
                sc.SaveChanges();
                TempData["Message"] = "<div class=\"alert alert-warning\" role=\"alert\">Giriş yapmadığınız için sorunuz onaylanana kadar gösterilmeyecektir</div>";
            }
            return Redirect(Url.Action("Index", "Home"));
        }
        public ActionResult Ara(string arananString)
        {
            return View();
        }

        [Route("Sorular/{baslik}/{SoruId}")]
        public ActionResult Soru(int? page) //Herhangi bir sorunun başlığına tıkladığımızda ya da url'den yazıp girdiğimizde çağırdığımız metoddur, soruya ilişkin detayları gösterir
        {
            int id = Convert.ToInt32(RouteData.Values["SoruId"]);

            if (sc.Sorular.Where(i => i.Id == id).FirstOrDefault() == null) return View("Hata", "_Layout", "<div class=\"alert alert-warning\" role=\"alert\">Soru bulunamadı</div>");
            sc.Sorular.Where(i => i.Id == id).FirstOrDefault().TiklanmaSayisi++;
            sc.SaveChanges();
            SoruCevapModel model = new SoruCevapModel(); //Bu metod için kullandığımız modelimiz(Ctrl'e basılı tutaral SoruCevapModel'e tıklarsanız içeriğini görebilirsiniz)
            model.Soru = sc.Sorular.Where(i => i.Id == id).FirstOrDefault();
            model.Soru.SoruSahibiPath = dc.Users.Where(i => i.UserName == model.Soru.SoruSahibi).FirstOrDefault()?.Path;
            model.SoruId = id;
            var user = dc.Users.Where(i => i.UserName == model.Soru.SoruSahibi).FirstOrDefault();
            model.SoruCevapContext = sc;
            model.SoruIzleyıcı = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            model.PagedList = sc.Cevaplar.Where(i => i.Soru.Id == id && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).ToPagedList(page ?? 1, 10);
            model.CevaplarModelForSoru = sc.Cevaplar.Where(i => i.Soru.Id == id && i.Onay == true && i.Soru.Onay == true).OrderBy(i => i.CevapTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
            model.CevapSayisi = sc.Cevaplar.Where(i => i.Soru.Id == id && i.Onay == true && i.Soru.Onay == true).Count();
            foreach (var item in model.CevaplarModelForSoru)
            {
                if (item.CevapSahibi == "Misafir") { continue; }
                item.CevapSahibiPath = dc.Users.Where(i => i.UserName == item.CevapSahibi).FirstOrDefault()?.Path;
            }
            if (User.Identity.IsAuthenticated)
            {
                if (dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault().FavoriSorular != null) //NullReferenceException
                {
                    foreach (var item in dc.FavoriSorular.Where(i => i.ApplicationUser.UserName == User.Identity.Name).ToList()) //giriş yapan kullanıcının, profiline girdiği kullanıcıyı önceden takip edip etmediğini anlamak ve view'imizi buna göre ayarlamak için kullandığımız bir kontrol(Takip Et ya da Takipten Çık)
                    {
                        if (item.FavoriSorular == id)
                        {
                            model.FavorideMi = true;
                        }
                    }
                }
            }
            return View(model);

        }
        public ActionResult SoruOyla(int soruId, string oy) //Soruları oylamak için kullandığımız metod
        {
            SoruCevapModel scm = new SoruCevapModel();
            scm.SoruCevapContext = sc;
            scm.SoruId = soruId;
            if (oy.Equals("arti")) //1-Eğer kullanıcı artı oy vermek istiyorsa
            {
                if (sc.EksiOy.Any(i => i.Soru.Id == soruId && i.EksiOySahibi == User.Identity.Name)) //1a-Kullanıcı daha önce eksi oy verdiyse
                {
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().ArtiOylar.Add(new ArtiOy //kullanıcının artı oyunu veritabanına at
                    {
                        ArtiOySahibi = User.Identity.Name
                    });
                    sc.EksiOy.Remove(sc.EksiOy.Where(i => i.Soru.Id == soruId && i.EksiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının eksi oyunu veritabanından sil
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger += 2; //Sorunun oyunu 2 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else if (sc.ArtiOy.Any(i => i.Soru.Id == soruId && i.ArtiOySahibi == User.Identity.Name)) //1b-Kullanıcı daha önce artı oy verdiyse
                {
                    sc.ArtiOy.Remove(sc.ArtiOy.Where(i => i.Soru.Id == soruId && i.ArtiOySahibi == User.Identity.Name).FirstOrDefault());//Kullanıcının artı oyunu veritabanından sil
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger -= 1; //Sorunun oyunu 1 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else //1c-Kullanıcı daha önce oy vermediyse
                {
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().ArtiOylar.Add(new ArtiOy //Kullanıcının artı oyunu veritabanına at
                    {
                        ArtiOySahibi = User.Identity.Name
                    });
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger += 1; //Sorunun oyunu 1 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }

            }
            else //2-Eğer kullanıcı eksi oy vermek istiyorsa
            {
                if (sc.ArtiOy.Any(i => i.Soru.Id == soruId && i.ArtiOySahibi == User.Identity.Name)) //2a-Kullanıcı daha önce artı oy verdiyse
                {
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().EksiOylar.Add(new EksiOy //Kullanıcının eksi oyunu veritabanına at
                    {
                        EksiOySahibi = User.Identity.Name
                    });
                    sc.ArtiOy.Remove(sc.ArtiOy.Where(i => i.Soru.Id == soruId && i.ArtiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının artı oyunu veritabanından sil
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger -= 2; //Sorunun oyunu 2 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else if (sc.EksiOy.Any(i => i.Soru.Id == soruId && i.EksiOySahibi == User.Identity.Name)) //2b-Kullanıcı daha önce eksi oy verdiyse
                {
                    sc.EksiOy.Remove(sc.EksiOy.Where(i => i.Soru.Id == soruId && i.EksiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının eksi oyunu veritabanından sil
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger += 1; //Sorunun değerini 1 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else //2c-Kullanıcı daha önce oy vermediyse
                {
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().EksiOylar.Add(new EksiOy //Kullanıcının eksi oyunu veritabanına at
                    {
                        EksiOySahibi = User.Identity.Name
                    });
                    sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault().Deger -= 1; //Sorunun değerini 1 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
            }
        }
        public ActionResult CevapOyla(int cevapId, string oy)
        {
            SoruCevapModel scm = new SoruCevapModel();
            scm.SoruCevapContext = sc;
            scm.CevapId = cevapId;
            if (oy.Equals("arti")) //1-Eğer kullanıcı artı oy vermek istiyorsa
            {
                if (sc.EksiOyCevaplar.Any(i => i.Cevap.Id == cevapId && i.EksiOySahibi == User.Identity.Name)) //1a-Kullanıcı daha önce eksi oy verdiyse
                {
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().ArtiOyCevaplar.Add(new ArtiOyCevaplar //kullanıcının artı oyunu veritabanına at
                    {
                        ArtiOySahibi = User.Identity.Name
                    });
                    sc.EksiOyCevaplar.Remove(sc.EksiOyCevaplar.Where(i => i.Cevap.Id == cevapId && i.EksiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının eksi oyunu veritabanından sil
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger += 2; //Sorunun oyunu 2 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else if (sc.ArtiOyCevaplar.Any(i => i.Cevap.Id == cevapId && i.ArtiOySahibi == User.Identity.Name)) //1b-Kullanıcı daha önce artı oy verdiyse
                {
                    sc.ArtiOyCevaplar.Remove(sc.ArtiOyCevaplar.Where(i => i.Cevap.Id == cevapId && i.ArtiOySahibi == User.Identity.Name).FirstOrDefault());//Kullanıcının artı oyunu veritabanından sil
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger -= 1; //Sorunun oyunu 1 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else //1c-Kullanıcı daha önce oy vermediyse
                {
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().ArtiOyCevaplar.Add(new ArtiOyCevaplar //Kullanıcının artı oyunu veritabanına at
                    {
                        ArtiOySahibi = User.Identity.Name
                    });
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger += 1; //Sorunun oyunu 1 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }

            }
            else //2-Eğer kullanıcı eksi oy vermek istiyorsa
            {
                if (sc.ArtiOyCevaplar.Any(i => i.Cevap.Id == cevapId && i.ArtiOySahibi == User.Identity.Name)) //2a-Kullanıcı daha önce artı oy verdiyse
                {
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().EksiOyCevaplar.Add(new EksiOyCevaplar //Kullanıcının eksi oyunu veritabanına at
                    {
                        EksiOySahibi = User.Identity.Name
                    });
                    sc.ArtiOyCevaplar.Remove(sc.ArtiOyCevaplar.Where(i => i.Cevap.Id == cevapId && i.ArtiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının artı oyunu veritabanından sil
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger -= 2; //Sorunun oyunu 2 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else if (sc.EksiOyCevaplar.Any(i => i.Cevap.Id == cevapId && i.EksiOySahibi == User.Identity.Name)) //2b-Kullanıcı daha önce eksi oy verdiyse
                {
                    sc.EksiOyCevaplar.Remove(sc.EksiOyCevaplar.Where(i => i.Cevap.Id == cevapId && i.EksiOySahibi == User.Identity.Name).FirstOrDefault()); //Kullanıcının eksi oyunu veritabanından sil
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger += 1; //Sorunun değerini 1 arttır
                    sc.SaveChanges();
                    return PartialView(scm);
                }
                else //2c-Kullanıcı daha önce oy vermediyse
                {
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().EksiOyCevaplar.Add(new EksiOyCevaplar //Kullanıcının eksi oyunu veritabanına at
                    {
                        EksiOySahibi = User.Identity.Name
                    });
                    sc.Cevaplar.Where(i => i.Id == cevapId).FirstOrDefault().Deger -= 1; //Sorunun değerini 1 azalt
                    sc.SaveChanges();
                    return PartialView(scm);
                }
            }
        }
        [HttpGet]
        public ActionResult SoruDuzenle(int soruId)
        {
            SorularModel soru = sc.Sorular.Where(i => i.Id == soruId).FirstOrDefault();
            IEnumerable<String> diller = new List<String>
            {
                "C#",
                "Java",
                "Asp.net",
                "Javascript",
                "Html"
            };
            ViewBag.Diller = diller;
            return View(soru);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult SoruDuzenle(SorularModel soru)
        {
            var eskiSoru = sc.Sorular.Where(i => i.Id == soru.Id).FirstOrDefault();
            eskiSoru.SoruBaslik = soru.SoruBaslik;
            eskiSoru.SoruIcerik = soru.SoruIcerik;
            eskiSoru.KodlamaDili = soru.KodlamaDili;
            sc.SaveChanges();
            TempData["Duzenlendi"] = "<div class=\"alert alert-success\" role=\"alert\">Sorunuz başarı ile düzenlendi</div>";
            return Redirect(Url.Action("Soru", "Sorular", new { baslik = FriendlyURL.FriendlyURLTitle(soru.SoruBaslik), SoruId = soru.Id }));
        }
        [HttpPost]
        public ActionResult SoruSil(SorularModel soru)
        {
            sc.ArtiOy.RemoveRange(sc.ArtiOy.Where(i => i.Soru.Id == soru.Id));
            sc.EksiOy.RemoveRange(sc.EksiOy.Where(i => i.Soru.Id == soru.Id));
            sc.ArtiOyCevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id).SelectMany(a => a.ArtiOyCevaplar));
            sc.EksiOyCevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id).SelectMany(a => a.EksiOyCevaplar));
            sc.Cevaplar.RemoveRange(sc.Cevaplar.Where(i => i.Soru.Id == soru.Id));
            sc.Sorular.Remove(sc.Sorular.Where(i => i.Id == soru.Id).FirstOrDefault());
            sc.SaveChanges();
            TempData["Message"] = "<div class=\"alert alert-success\" role=\"alert\">Sorunuz başarı ile silindi</div>";
            return Redirect(Url.Action("Index", "Home"));
        }
        [Authorize(Roles = "admin, editor")]
        public ActionResult HaberEkle()
        {

            return View();
        }
        [Authorize(Roles = "admin, editor")]
        [HttpPost]
        public ActionResult HaberEkle(YeniHaberler model)
        {
            ViewBag.HaberMessage = "<div class=\"alert alert-success\" role=\"alert\">Haber başarı ile eklendi</div>";
            sc.YeniHaberler.Add(model);
            sc.SaveChanges();
            return View();
        }
        public PartialViewResult Haberler()
        {
            var model = sc.YeniHaberler.ToList();

            return PartialView(model);
        }
    }
}
