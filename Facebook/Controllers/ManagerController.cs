using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Facebook.Models;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;
using DataAccess;
using System.Net.Http;

namespace Facebook.Controllers
{
    public class ManagerController : Controller
    {
        private facebookEntities db = new facebookEntities();
        List<tbl_data> resultObject = new List<tbl_data>();
        // GET: Manager
        public ActionResult Index(int? id)
        {
            try 
            {
                List<tbl_data> addResultObject = new List<tbl_data>();
                List<tbl_data> notCheckObject = new List<tbl_data>();
                List<tbl_data> addResultObjectForUser = new List<tbl_data>();
                List<tbl_data> previousForUser = new List<tbl_data>();
                
                //최고관리자 정보
                var admin = db.tbl_userlist.Where(a => a.role == 0).FirstOrDefault();
                //표시되는 유저 정보
                var user = db.tbl_userlist.Where(a => a.id == id).FirstOrDefault();
                //디비에서 이전 자료 얻기
                var userlist = db.tbl_userlist.ToList();
                var previous = db.tbl_data.ToList();

                resultObject.RemoveRange(0, resultObject.Count);

                Thread[] threadArray = new Thread[userlist.Count]; 
                for(int i = 0; i < userlist.Count; i++)
                {
                    threadArray[i] = null;
                }
                //새 자료 얻기
                int num = 0;
                foreach (var item in userlist)
                {
                    threadArray[num] = new Thread(new ThreadStart(() => GetClientId(item.id)));
                    threadArray[num].Start();
                    num ++;
                }

                for (int i = 0; i < userlist.Count; i++)
                {
                    threadArray[i].Join();
                }

                // 이전 정보 새 정보 대비로 새로 추가된 정보 얻기
                for (int i = 0; i < resultObject.Count; i++)
                {
                    bool IsExistCheck = false;
                    foreach (var item in previous)
                    {
                        if (Convert.ToDateTime(resultObject[i].created_time).Ticks == Convert.ToDateTime(item.created_time).Ticks && resultObject[i].phone == item.phone && resultObject[i].source == item.source)
                        {
                            IsExistCheck = true;
                        }
                    }
                    if (!IsExistCheck)
                    {
                        addResultObject.Add(resultObject[i]);
                    }
                }

                foreach (var item in previous)
                {
                    if (item.check == 2)
                    {
                        notCheckObject.Add(item);
                    }
                }

                for (int l = 0; l < notCheckObject.Count; l++)
                {
                    addResultObject.Add(notCheckObject[l]);
                    previous.Remove(notCheckObject[l]);
                }

                for (int k = 0; k < addResultObject.Count; k++)
                {
                    if (addResultObject[k].uid == user.id)
                    {
                        addResultObjectForUser.Add(addResultObject[k]);
                    }
                }

                for (int k = 0; k < previous.Count; k++)
                {
                    if (previous[k].uid == user.id)
                    {
                        previousForUser.Add(previous[k]);
                    }
                }

                ViewBag.addResult = addResultObject;
                ViewBag.previous = previous;
                ViewBag.addResultForUser = addResultObjectForUser;
                ViewBag.previousForUser = previousForUser;
                ViewBag.admin = admin;
                ViewBag.user = user;

                return View(db.tbl_userlist.ToList());
            }
            catch(Exception ex)
            {
                return Content("최고관리자페이지 로딩 오류입니다.<br>" + ex);
            }
        }

        public bool start()
        {
            HttpClientHandler cookiehandler = new HttpClientHandler();
            HttpClient cookiehttpClient = new HttpClient(cookiehandler);
            List<KeyValuePair<string, string>> cookieinputs = new List<KeyValuePair<string, string>>();
            cookieinputs.Add(new KeyValuePair<string, string>("id", "2"));
            HttpResponseMessage cookieresponseMessageMain = cookiehttpClient.PostAsync("http://144.48.222.110:5555/bot", (HttpContent)new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)cookieinputs)).Result;

            cookieresponseMessageMain.EnsureSuccessStatusCode();
            string cookieresponseMessageLoginString = cookieresponseMessageMain.Content.ReadAsStringAsync().Result;
            Console.WriteLine(cookieresponseMessageLoginString);

            string[] cookie = cookieresponseMessageLoginString.Split(',');
            List<string> cookieList = new List<string>();
            for (int i = 0; i < cookie.Length; i++)
            {
                string[] detail = cookie[i].Split(':');
                cookieList.Add(detail[1].Split('"')[1]);
                Console.WriteLine(detail[1].Split('"')[1]);
            }

            string enable = cookieList[0];
            if (enable != "enable")
            {
                return false;
            }
            return true;
        }
        public void GetClientId(int id)
        {
            try
            {
                if (!start()) return;

                var user = db.tbl_userlist.Where(a => a.id == id).FirstOrDefault();
                string url = "https://graph.facebook.com/" + user.page_id + "/leadgen_forms?access_token=" + user.token;
                //string url = "http://localhost:5555/bot/" + id;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "text/json";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    JObject jsonData = JObject.Parse(result);
                    var d = jsonData["data"];

                    Thread[] threadArray = new Thread[jsonData["data"].Count()];
                    for (int i = 0; i < jsonData["data"].Count(); i++)
                    {
                        threadArray[i] = null;
                    }

                    for (var i = 0; i < jsonData["data"].Count(); i++)
                    {
                        string clientId = jsonData["data"][i]["id"].ToString();
                        string source = jsonData["data"][i]["name"].ToString();
                        string status = jsonData["data"][i]["status"].ToString();

                        threadArray[i] = new Thread(new ThreadStart(() => GetData(id, clientId, user.token, source, status)));
                        threadArray[i].Start();
                    }

                    for (int i = 0; i < jsonData["data"].Count(); i++)
                    {
                        threadArray[i].Join();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("광고 정보 얻기 오류입니다.<br>" + ex);
            }
        }
        public void GetData(int id, string clientId, string token, string source, string status)
        {
            try
            {
                string url = "https://graph.facebook.com/" + clientId + "/leads?access_token=" + token;
                //string url = "http://localhost:5555/detail/" + id;

                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url); ;
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.ContentType = "text/json";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var result = "";
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                    JObject jsonData = JObject.Parse(result);
                    for (var i = 0; i < jsonData["data"].Count(); i++)
                    {
                        tbl_data data = new tbl_data();
                        data.created_time = jsonData["data"][i]["created_time"].ToString();
                        data.name = "";
                        data.phone = "";
                        data.email = "";
                        data.status = "";
                        data.source = "";
                        for (int j = 0; j < jsonData["data"][i]["field_data"].Count(); j++)
                        {
                            if (jsonData["data"][i]["field_data"][j]["name"].ToString() == "이름" || jsonData["data"][i]["field_data"][j]["name"].ToString().Contains("name"))
                            {
                                data.name = jsonData["data"][i]["field_data"][j]["values"][0].ToString();
                            }
                            if (jsonData["data"][i]["field_data"][j]["name"].ToString() == "전화번호" || jsonData["data"][i]["field_data"][j]["name"].ToString().Contains("phone"))
                            {
                                data.phone = jsonData["data"][i]["field_data"][j]["values"][0].ToString();
                            }

                            if (jsonData["data"][i]["field_data"][j]["name"].ToString().Contains("이메일") || jsonData["data"][i]["field_data"][j]["name"].ToString().Contains("email"))
                            {
                                data.email = jsonData["data"][i]["field_data"][j]["values"][0].ToString();
                            }
                        }
                        data.status =  status;
                        data.source = source;
                        data.uid = id;
                        if(!data.name.Contains("dummy") && !data.phone.Contains("dummy"))
                            resultObject.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("잠재고객데이터 얻기 오류입니다.<br>" + ex);
            }
        }

        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tbl_userlist userList = db.tbl_userlist.Find(id);
                userList.password = CryptSHA256.Decrypt(userList.password);

                if (userList == null)
                {
                    return HttpNotFound();
                }
                ViewBag.id = id;
                return View(userList);
            }
            catch(Exception ex)
            {
                return Content("유저 편집 페이지 로딩 오류입니다.<br>" + ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,email,password,role,page_id,token,name")] tbl_userlist userList, string time)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    userList.password = CryptSHA256.Encrypt(userList.password);
                    userList.time = time;
                    db.Entry(userList).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Manager", new { id = userList.id });
                }
                return View(userList);
            }
            catch(Exception ex)
            {
                return Content("유저 편집 오류입니다. 유저 정보를 바로 입력해주세요. <br>" + ex);
            }
        }

        public ActionResult Create(int? id)
        {
            try
            {
                ViewBag.id = id;
                tbl_userlist userList = db.tbl_userlist.Find(id);
                ViewBag.name = userList.name;
                return View();
            }
            catch(Exception ex)
            {
                return Content("유저 창조 페이지 로딩 오류입니다.<br>" + ex);
            }
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,email,password,role,page_id,token,name")] tbl_userlist userList, string time)
        {
            try
            {
                userList.password = CryptSHA256.Encrypt(userList.password);
                userList.time = time;
                if (ModelState.IsValid)
                {
                    db.tbl_userlist.Add(userList);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Manager", new { id = Convert.ToInt32(Session["id"]) });
                }
                return View(userList);
            }
            catch(Exception ex)
            {
                return Content("유저 창조 오류입니다. 유저 정보를 정확히 입력해주세요.<br>" + ex);
            }
        }

        public ActionResult Delete(int id)
        {
            try
            {
                tbl_userlist tbl_userlist = db.tbl_userlist.Find(id);
                db.tbl_userlist.Remove(tbl_userlist);
                db.SaveChanges();
                return RedirectToAction("Index", "Manager", new { id = Convert.ToInt32(Session["id"])});
            }
            catch(Exception ex)
            {
                return Content("유저삭제 오류입니다.<br>" + ex);
            }
        }

        public string Check(string uid, string created_time, string name, string phone, string status, string source)
        {
            try
            {
                var userList = db.tbl_data.ToList();
                tbl_data user = new tbl_data();

                bool IsExistCheck = false;
                for (int i = 0; i < userList.Count; i++)
                {
                    if (Convert.ToDateTime(userList[i].created_time).Ticks == Convert.ToDateTime(created_time).Ticks && userList[i].phone == phone && userList[i].source == source)
                    {
                        IsExistCheck = true;
                        user = userList[i];
                    }
                }
                
                if (IsExistCheck)
                {
                    user.check = 3;
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return "edit success: " + user.check + " name: " + user.name +" phone: " + user.phone + " source: " + user.source;

                }
                else
                {
                    tbl_data data = new tbl_data();
                    data.uid = Convert.ToInt32(uid);
                    data.created_time = created_time;
                    data.name = name;
                    data.phone = phone;
                    data.status = status;
                    data.source = source;
                    data.check = 1;
                    db.tbl_data.Add(data);
                    db.SaveChanges();
                    return "create success";
                }
            }
            catch (Exception ex)
            {
                return "false";
            }
        }

        public bool LoginCheck()
        {
            try
            {
                if (Session["email"] != "" && Session["email"] != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}