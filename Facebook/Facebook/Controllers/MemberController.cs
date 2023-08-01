using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Facebook.Models;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Threading;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace Facebook.Controllers
{
    public class MemberController : Controller
    {
        private facebookEntities db = new facebookEntities();
        // GET: Member
        public ActionResult Index(int? id)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                CookieContainer container = new CookieContainer();
                handler.CookieContainer = container;
                HttpClient httpClient = new HttpClient(handler);

                var user = db.tbl_userlist.Where(a => a.id == id).FirstOrDefault();
                string url = "https://graph.facebook.com/" + user.page_id + "/leadgen_forms?access_token=" + user.token;
                //HttpResponseMessage responseMessageMain = httpClient.GetAsync(url).Result;

                ViewBag.name = user.name;
                return View();
            }
            catch (Exception ex)
            {
                return View();
            }
        }
    }
}