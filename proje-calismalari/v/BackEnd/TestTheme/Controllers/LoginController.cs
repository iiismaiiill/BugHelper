using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TestTheme.Models;

namespace TestTheme.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Autherize(TestTheme.Models.LoginDB loginModel)
        {
            using (LoginEntities db = new LoginEntities())
            {
                var userDetails = db.LoginDB.Where(x => x.username == loginModel.username && x.password == loginModel.password).FirstOrDefault();
                if (userDetails == null)
                {
                    loginModel.LoginErrorMessage = "Hatalı kullanıcı adı veya parola";
                    return View("Login", loginModel);
                }
                else
                {
                    Session["userID"] = userDetails.UserID;
                    Session["username"] = userDetails.username;
                    return RedirectToAction("Index", "Default");
                }
            }
        }
        public ActionResult LogOut()
        {
            int userID = (int)Session["userID"];
            Session.Abandon();
            return RedirectToAction("Login", "Login");
        }
        public ActionResult Register()
        {
                return View();
        }
        [HttpPost]
        public ActionResult Register(LoginDB obj)
        {
            if (ModelState.IsValid)
            {
                LoginEntities db = new LoginEntities();
                db.LoginDB.Add(obj);
                db.SaveChanges();
            }
            return View(obj);
        }
        public ActionResult ForgetPassword()
        {
            return View();
        }
        public ActionResult ResetPassword()
        {
            return View();
        }
    }
}