using fstCopy_Proj5.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace fstCopy_Proj5.Controllers
{
    public class Debates1Controller : Controller
    {
        private ElectionEntities1 db = new ElectionEntities1();

        // GET: Debates1
        public ActionResult Index()
        {
            return View(db.Debates.ToList());
        }
        // GET: Debates1
        public ActionResult IndexDebatsesHome()
        {
            return View(db.Debates.ToList());
        }
        // GET: Debates1/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Debate debate = db.Debates.Find(id);
            if (debate == null)
            {
                return HttpNotFound();
            }
            return View(debate);
        }

        // GET: Debates1/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Debates1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Circle_ID,Date_Of_Debate,Topic,First_Candidate,First_Candidate_List,Second_Candidate,Second_Candidate_List,Status,Zoom_link")] Debate debate)
        {
            if (ModelState.IsValid)
            {
                db.Debates.Add(debate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(debate);
        }

        // GET: Debates1/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Debate debate = db.Debates.Find(id);
            if (debate == null)
            {
                return HttpNotFound();
            }
            return View(debate);
        }

        // POST: Debates1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Circle_ID,Date_Of_Debate,Topic,First_Candidate,First_Candidate_List,Second_Candidate,Second_Candidate_List,Status,Zoom_link")] Debate debate)
        {

            var x1 = db.Debates.Where(x => x.ID == debate.ID).FirstOrDefault();
            x1.Status = debate.Status;
            x1.Zoom_link = debate.Zoom_link;

            if (ModelState.IsValid)
            {
                db.Entry(x1).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(debate);
        }

        // GET: Debates1/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Debate debate = db.Debates.Find(id);
            if (debate == null)
            {
                return HttpNotFound();
            }
            return View(debate);
        }

        // POST: Debates1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Debate debate = db.Debates.Find(id);
            db.Debates.Remove(debate);
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