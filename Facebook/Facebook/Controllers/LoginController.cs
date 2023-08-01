using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Facebook.Models;

namespace Facebook.Controllers
{
    public class LoginController : Controller
    {
        private facebookEntities db = new facebookEntities();
        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(tbl_userlist user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var sdf = db.tbl_userlist.ToList();
                    var obj = db.tbl_userlist.Where(a => a.email == user.email);

                    if (obj.Any())
                    {
                        foreach (var item in obj)
                        {
                            if (item.password == user.password)
                            {
                                Session["email"] = user.email.ToString();
                                Session["password"] = user.password.ToString();
                                if (item.role == 0)
                                {
                                    return RedirectToAction("Index", "Manager", new { email = user.email.ToString() });
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
                return View();
            }
        }
    }
}