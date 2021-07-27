using BugHelper.Identity;
using BugHelper.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static BugHelper.Models.SorularPaged;

namespace BugHelper.Controllers
{
    public class HomeController : Controller
    {
        private SorularContext sc = new SorularContext();
        private IdentityDataContext dc = new IdentityDataContext();
        SorularPaged model = new SorularPaged();
        [RequireHttps]
        public ActionResult Index(int? page) //kullanıcı herhangi bir filtreleme işlemi talep etmedi ise gözükecek soruları, onaylanmış sorulardan tarihe göre azalan şekilde liste yaparak gönderiyoruz
        {
            Random rnd = new Random();
            if (User.Identity.IsAuthenticated) {
                model.SoruIzleyici = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            }
            model.PagedList = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
            model.Sorular = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
            model.SoruSayisi = sc.Sorular.Where(i => i.Onay == true).Count();
            model.Etiketler = new List<EtiketListesi>();
            foreach (var item in sc.Sorular.Where(i => i.Onay == true).Select(a => a.KodlamaDili).Distinct().ToList())
            {
                model.Etiketler.Add(new EtiketListesi
                {
                    Etiket = item,
                    SoruSayisi = sc.Sorular.Where(i => i.KodlamaDili == item).Count()
                });
            }
            
            string[] temp = new string[model.Etiketler.Count()];
            string[] renkler = { "default", "primary", "secondary", "danger", "dark", "info", "success" };
            for (int i = 0; i < model.Etiketler.Count(); i++)
            {
                int index = rnd.Next(renkler.Count());
                temp[i] = renkler[index];
            }
            model.EtiketRenkleri = temp;

            foreach (var item in model.Sorular)
            {
                if(item.SoruSahibi == "Misafir") { continue; }
                item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault()?.Path;
            }
            return View(model);
        }
        public ActionResult Ara(string arananString, int? page) //onaylı olan sorulardan, başlığında veya içeriğinde aranan string'i barındıran soruları liste halinde gönderiyoruz
        {
            if (User.Identity.IsAuthenticated)
            {
                model.SoruIzleyici = dc.Users.Where(i => i.UserName == User.Identity.Name).FirstOrDefault();
            }
            model.PagedList = sc.Sorular.Where(i => i.SoruBaslik.Contains(arananString) || i.SoruIcerik.Contains(arananString) && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
            model.Sorular = sc.Sorular.Where(i => i.SoruBaslik.Contains(arananString) || i.SoruIcerik.Contains(arananString) && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
            model.SoruSayisi = sc.Sorular.Count();
            foreach (var item in model.Sorular)
            {
                if (item.SoruSahibi == "Misafir") { continue; }
                item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
            }
            return View("Index",model);
        }
        [HttpPost]
        public PartialViewResult SorularFiltre(string etiketFiltre, int? page)//burada da benzer filteleme işlemlerini, kullanıcının taleplerine göre uygulayıp gönderiyoruz
        {
            foreach (var etiket in sc.Etiketler.ToList())
            {
                if (etiketFiltre.Equals(etiket.KodlamaDili))
                {
                    model.PagedList = sc.Sorular.Where(i => i.KodlamaDili == etiket.KodlamaDili && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                    model.Sorular = sc.Sorular.Where(i => i.KodlamaDili == etiket.KodlamaDili && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                    foreach (var item in model.Sorular)
                    {
                        if (item.SoruSahibi == "Misafir") { continue; }
                        item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
                    }
                    return PartialView(model);
                }
            }
            if(etiketFiltre.Equals("Cevapsızlar")){
                model.PagedList = sc.Sorular.Where(i => i.CevapSayisi == 0 && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                model.Sorular = sc.Sorular.Where(i => i.CevapSayisi == 0 && i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                foreach (var item in model.Sorular)
                {
                    if (item.SoruSahibi == "Misafir") { continue; }
                    item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
                }
                return PartialView(model);
            }
            else if(etiketFiltre.Equals("Beğenilenler"))
            {
                model.PagedList = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.Deger).ToPagedList(page ?? 1, 10);
                model.Sorular = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.Deger).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                foreach (var item in model.Sorular)
                {
                    if (item.SoruSahibi == "Misafir") { continue; }
                    item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
                }
                return PartialView(model);
            }
            else if (etiketFiltre.Equals("Pasif"))
            {
                model.PagedList = sc.Sorular.Where(i => i.Onay == false).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                model.Sorular = sc.Sorular.Where(i => i.Onay == false).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                foreach (var item in model.Sorular)
                {
                    if (item.SoruSahibi == "Misafir") { continue; }
                    item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
                }
                return PartialView(model);
            }
            else
            {
                model.PagedList = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).ToPagedList(page ?? 1, 10);
                model.Sorular = sc.Sorular.Where(i => i.Onay == true).OrderByDescending(i => i.SorulmaTarihi).Skip((page - 1 ?? 0) * 10).Take(10).ToList();
                foreach (var item in model.Sorular)
                {
                    if (item.SoruSahibi == "Misafir") { continue; }
                    item.SoruSahibiPath = dc.Users.Where(i => i.UserName == item.SoruSahibi).FirstOrDefault().Path;
                }
                return PartialView(model);
            }
        }
        public ActionResult Hakkimizda()
        {
            return View();
        }
        public ActionResult Iletisim()
        {
            return View();
        }
    }
}