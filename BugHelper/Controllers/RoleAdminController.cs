using BugHelper.Identity;
using BugHelper.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BugHelper.Controllers
{
    [Authorize(Roles = "admin, editor")]
    public class RoleAdminController : Controller //Rolleri oluşturmak, silmek ve editlemek için kullandığımız sınıf
    {
        private RoleManager<IdentityRole> roleManager;
        private UserManager<ApplicationUser> userManager;
        private SorularContext sc = new SorularContext();
        private IdentityDataContext dc = new IdentityDataContext();
        public RoleAdminController()
        {
           roleManager=new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new IdentityDataContext())); //Rol işlemleri için kullanacağımız roleManager'imiz, genel olarak bu manager'lerin kaynağı IdentityDataContext'imiz
            userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new IdentityDataContext()));
        }
        public ActionResult Index()
        {
            return View(roleManager.Roles);
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(String name) //AccountController'deki register ile pek farkı yok
        {
            if (ModelState.IsValid)
            {
                var result = roleManager.Create(new IdentityRole(name)); //roleManager rol oluşturmak için Create ile yeni bir IdentityRole("rolümüzün adı") nesnesi oluşturup Create'e gönderip oluşturuyoruz
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var item in result.Errors)
                    {
                        ModelState.AddModelError("", item);
                    }
                }
            }
            return View(name);
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            var role = roleManager.FindById(id);

            if (role != null)
            {
                var result = roleManager.Delete(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View("Hata", result.Errors);
                }
            }
            else
            {
                return View("Hata", new string[] { "Rol Bulunamadı" });
            }
        }

        [HttpGet]
        public ActionResult Edit(string id) //bu metod ile oluşturduğumuz role atayacağımız ya da rolden çıkaracağımız kullanıcıların listesini members(role sahip olanlar) ve nonmembers(role sahip olmayanlar) olarak bastırıyoruz(Uygulamayı çalıştırıp localhost:...../roleadmin gidip rol oluşturduktan sonra güncelleye basarsanız görebilirsiniz).
        {
            var role = roleManager.FindById(id); //IdentityRol referansı oluşturup bu referanse gelen id ile bulduğumuz rolü gösterdik(var tanımının üzerine gelirseniz hangi sınıfı kullandığını görebilirsiniz(bkz.IdentityRole).).

            var members = new List<ApplicationUser>(); //üyelerin listesi
            var nonMembers = new List<ApplicationUser>(); //üye olmayanların listesi

            foreach (var user in userManager.Users.ToList()) //kullanıcılar içerisinde ara
            {
                var list = userManager.IsInRole(user.Id, role.Name) ? //aradığın her kullanıcı için; eğer kullanıcı gelen id değerindeki role sahipse(satır 78) members(üye)'ı list'e at, değilse nonmembers(üye olmayanlar)'ı list'e at
                    members : nonMembers;

                list.Add(user);
            }

            return View(new RoleEditModel()
            {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }

        [HttpPost]
        public ActionResult Edit(RoleUpdateModel model) //bu metod ile önceki Edit metodunda bastırdığımız kullanıcılardan seçtiklerimize rolleri atayıp güncelliyoruz)
        {
            IdentityResult result;

            if (ModelState.IsValid)
            {
                foreach (var userId in model.IdsToAdd ?? new string[] { }) // ?? coalescing operatörüdür(eğer ??'den önceki değer null ise ??'den sonraki değer atanır)(bkz.NullReferenceException)
                {
                    result = userManager.AddToRole(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Hata", result.Errors);
                    }
                }

                foreach (var userId in model.IdsToDelete ?? new string[] { })
                {
                    result = userManager.RemoveFromRole(userId, model.RoleName);
                    if (!result.Succeeded)
                    {
                        return View("Hata", result.Errors);
                    }
                }
                return RedirectToAction("Index");
            }

            return View("Hata", new string[] { "aranılan rol yok." });
        }
        public ActionResult EtiketEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult EtiketEkle(Etiket etiket)
        {
            sc.Etiketler.Add(etiket);
            sc.SaveChanges();
            ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Etiket başarı ile eklendi</div>";
            return View();
        }
        public ActionResult UlkeEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UlkeEkle(UlkelerModel ulke)
        {
            dc.Ulkeler.Add(ulke);
            dc.SaveChanges();
            ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Ülke başarı ile eklendi</div>";
            return View();
        }
    }
}
