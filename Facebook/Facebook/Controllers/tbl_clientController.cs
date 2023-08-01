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
    public class tbl_clientController : Controller
    {
        private facebookEntities db = new facebookEntities();

        // GET: tbl_client
        public ActionResult Index()
        {
            return View(db.tbl_client.ToList());
        }

        // GET: tbl_client/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_client tbl_client = db.tbl_client.Find(id);
            if (tbl_client == null)
            {
                return HttpNotFound();
            }
            return View(tbl_client);
        }

        // GET: tbl_client/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: tbl_client/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,pid,client_id,created_time")] tbl_client tbl_client)
        {
            if (ModelState.IsValid)
            {
                db.tbl_client.Add(tbl_client);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tbl_client);
        }

        // GET: tbl_client/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_client tbl_client = db.tbl_client.Find(id);
            if (tbl_client == null)
            {
                return HttpNotFound();
            }
            return View(tbl_client);
        }

        // POST: tbl_client/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,pid,client_id,created_time")] tbl_client tbl_client)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tbl_client).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tbl_client);
        }

        // GET: tbl_client/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tbl_client tbl_client = db.tbl_client.Find(id);
            if (tbl_client == null)
            {
                return HttpNotFound();
            }
            return View(tbl_client);
        }

        // POST: tbl_client/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tbl_client tbl_client = db.tbl_client.Find(id);
            db.tbl_client.Remove(tbl_client);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
