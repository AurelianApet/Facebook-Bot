using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Facebook.Models;
using DataAccess;

namespace Facebook.Controllers
{
    public class LoginController : Controller
    {
        private facebookEntities db = new facebookEntities();
        // GET: Login
        public ActionResult Index()
        {
            try
            {
                Session["email"] = "";
                Session["password"] = "";
                Session["name"] = "";

                return View();
            }
            catch(Exception ex)
            {
                return Content("로그인페지 로딩 오류입니다.<br>" + ex);
            }
        }

        [HttpPost]
        public ActionResult Index([Bind(Include = "email,password,save")] Login  user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var sdf = db.tbl_userlist.ToList();
                    var obj = db.tbl_userlist.Where(a => a.email == user.email);
                    Session["save"] = "save";

                    if (obj.Any())
                    {
                        foreach (var item in obj)
                        {
                            if (item.password == CryptSHA256.Encrypt(user.password))
                            {
                                Session["email"] = user.email.ToString();
                                Session["password"] = user.password.ToString();
                                Session["name"] = item.name.ToString();
                                Session["id"] = item.id.ToString();
                                if (item.role == 0)
                                {
                                    return RedirectToAction("Index", "Manager", new { id = Convert.ToInt32(item.id)});
                                }
                                else
                                {
                                    return RedirectToAction("Index", "Member", new { id = Convert.ToInt32(item.id)});
                                }
                            }
                        }
                    }
                    else
                    {
                        return View();
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                return Content("로그인요청 오류입니다.<br>" + ex);
            }
        }
    }
}