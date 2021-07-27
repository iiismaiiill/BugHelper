using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestTheme.Models;

namespace TestTheme.Controllers
{
    public class ProfilController : Controller
    {
        // GET: Profil
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult FotoGuncelle()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult FotoGuncelle(HttpPostedFileBase file)
        {
            string mesaj;
            decimal DosyaBoyutu = 500;
            try
            {
                var desteklenenTip = new[] { "jpg", "jpeg", "png" };
                var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                if (!desteklenenTip.Contains(fileExt))
                {
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Geçersiz dosya formatı. Yükleyeceğiniz fotoğraf jpg veya png olmalıdır.</div>";
                    return View();
                }
                 else if (file.ContentLength > (DosyaBoyutu * 1024))
                {
                    mesaj = "Dosya boyutu en fazla " + DosyaBoyutu + "KB olmalıdır!";
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">"+mesaj+"</div>";
                    
                    return View();
                }
                else
                {
                    var _FileName = Path.GetFileName(file.FileName);
                    var _path = Path.Combine(Server.MapPath("~/img"), _FileName);
                    file.SaveAs(_path);

                    ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Yükleme başarılı</div>";
                    return View();
                }
            }
            catch
            {
                ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">Hata Oluştu. Tekrar Deneyiniz.</div>";
                return View();
            }
        }
        [HttpGet]
        public ActionResult ProfilDuzenle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ProfilDuzenle(ProfilGun model)
        {
            ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">Profil güncelleme başarılı...</div>";
            return View();
        }
        [HttpGet]
        public ActionResult EpostaDegistir()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EpostaDegistir(EmailVal model)
        {
            string mail = "abc@mail.com";

            if(model.CurrentEmail == null || model.NewEmail == null || model.ConfirmEmail == null)
            {
                ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\"> E-Posta alanları boş bırakılamaz...</div>";
                
                return View();
            }

            if (model.CurrentEmail == mail)
            {
                if (model.NewEmail == model.ConfirmEmail)
                {
                    ViewBag.Message = "<div class=\"alert alert-success\" role=\"alert\">E-Posta güncelleme başarılı...</div>";
                    return View();
                }
                else
                {
                    ViewBag.Message = "<div class=\"alert alert-warning\" role=\"alert\">E-Postalar uyuşmuyor...</div>";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "<div class=\"alert alert-danger\" role=\"alert\">Varsayılan E-Posta hatalı...</div>";
                return View();
            }
        }
        public ActionResult ParolaDegistir()
        {
            return View();
        }
        public ActionResult TwoFactor()
        {
            return View();
        }
        public ActionResult UyelikIptal()
        {
            return View();
        }
    }
}