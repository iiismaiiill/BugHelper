using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestTheme.Models;

namespace TestTheme.Controllers
{
    public class SoruCevapController : Controller
    {
        [HttpGet]
        public ActionResult SoruYaz()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SoruYaz(Sorular model)
        {
            var gelenicerik = model.SoruIcerik;
            ViewBag.message = gelenicerik.ToString();
            return View();
        }
    }
}