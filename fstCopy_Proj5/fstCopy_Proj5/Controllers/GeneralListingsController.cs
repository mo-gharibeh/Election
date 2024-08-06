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
    public class GeneralListingsController : Controller
    {
        private ElectionEntities1 db = new  ElectionEntities1();

        // GET: GeneralListings
        public ActionResult Index()
        {
            return View(db.GeneralListings.ToList());
        }

        // GET: GeneralListings/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GeneralListing generalListing = db.GeneralListings.Find(id);
            if (generalListing == null)
            {
                return HttpNotFound();
            }
            return View(generalListing);
        }

        // GET: GeneralListings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: GeneralListings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "GeneralListingID,Name,NumberOfVotes")] GeneralListing generalListing)
        {
            if (ModelState.IsValid)
            {
                db.GeneralListings.Add(generalListing);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(generalListing);
        }

        // GET: GeneralListings/Edit/5
        public ActionResult Edit(int id)
        {
            var candidate = db.GeneralListCandidates.Find(id);
            if (candidate == null)
            {
                return HttpNotFound();
            }

            // Create a SelectList for Status
            ViewBag.StatusList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Reject", Value = "0" },
                new SelectListItem { Text = "Accept", Value = "1" }
            }, "Value", "Text", candidate.Status);

            return View(candidate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GeneralListCandidate candidate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(candidate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Re-populate the SelectList in case of error
            ViewBag.StatusList = new SelectList(new List<SelectListItem>
            {
                new SelectListItem { Text = "Reject", Value = "0" },
                new SelectListItem { Text = "Accept", Value = "1" }
            }, "Value", "Text", candidate.Status);

            return View(candidate);
        }

        // GET: GeneralListings/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GeneralListing generalListing = db.GeneralListings.Find(id);
            if (generalListing == null)
            {
                return HttpNotFound();
            }
            return View(generalListing);
        }

        // POST: GeneralListings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            GeneralListing generalListing = db.GeneralListings.Find(id);
            db.GeneralListings.Remove(generalListing);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteListingWithCandidates(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var generalListing = db.GeneralListings.Include(gl => gl.GeneralListCandidates).FirstOrDefault(gl => gl.Name == id);

            if (generalListing == null)
            {
                return HttpNotFound();
            }

            // Remove the general listing (cascades to associated candidates)
            db.GeneralListings.Remove(generalListing);
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