using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace fstCopy_Proj5.Models
{
    public class LocalListCandidatesController : Controller
    {
        private ElectionEntities db = new ElectionEntities();


        public ActionResult Index()
        {
            try
            {
                
                var candidates = db.LocalListCandidates
                    .Where(c => c.LocalListingID != null)
                    .ToList();

                return View(candidates);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء جلب البيانات: " + ex.Message);
                return View(new List<LocalListCandidate>());
            }
        }








        public ActionResult CreateList()
        {
            PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateList(LocalList localList)
        {
            if (ModelState.IsValid)
            {

                try
                {
                    db.LocalLists.Add(localList);
                    db.SaveChanges();
                    TempData["LocalListID"] = localList.ID;
                    TempData["Governorate"] = localList.Governorate;
                    TempData["ElectionArea"] = localList.ElectionArea;
                    return RedirectToAction("Create");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            PopulateDropDowns();
            return View(localList);
        }
        public ActionResult Create()
        {
            ViewBag.LocalListID = TempData["LocalListID"];
            ViewBag.Governorate = TempData["Governorate"];
            ViewBag.ElectionArea = TempData["ElectionArea"];
            PopulateDropDowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CandidateID,CandidateName,NationalNumber,Gender,Religion,Type_Chair,BirthdateCandidate,Governorate,ElectionArea,NumberOfVotesCandidate,LocalListingID")] LocalListCandidate localListCandidate)
        {
            if (ModelState.IsValid)
            {
                if (TempData["LocalListID"] != null)
                {
                    localListCandidate.LocalListingID = (int)TempData["LocalListID"];
                }

                if (!string.IsNullOrEmpty(localListCandidate.BirthdateCandidate))
                {
                    DateTime birthDate;
                    if (DateTime.TryParse(localListCandidate.BirthdateCandidate, out birthDate))
                    {
                        int age = DateTime.Now.Year - birthDate.Year;
                        if (birthDate > DateTime.Now.AddYears(-age)) age--;

                        if (age >= 25)
                        {
                            localListCandidate.Status = "Accepted";
                            db.LocalListCandidates.Add(localListCandidate);
                            db.SaveChanges();
                           // return RedirectToAction("CreateCandidates", new { listId = localListCandidate.LocalListingID });
                            return RedirectToAction("CreateCandidates", new { listId = localListCandidate.LocalListingID });
                        }
                        else
                        {
                            localListCandidate.Status = "Rejected";
                            ModelState.AddModelError("", "العمر يجب أن يكون 25 سنة أو أكثر.");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("BirthdateCandidate", "تنسيق تاريخ غير صحيح.");
                    }
                }
                else
                {
                    ModelState.AddModelError("BirthdateCandidate", "تاريخ الميلاد مطلوب.");
                }
            }

            ViewBag.LocalListID = localListCandidate.LocalListingID;
            ViewBag.Governorate = TempData["Governorate"];
            ViewBag.ElectionArea = TempData["ElectionArea"];
            PopulateDropDowns(localListCandidate);
            return View(localListCandidate);
        }

        public ActionResult CreateCandidates(int listId)
        {
            TempData["LocalListID"] = listId;
            ViewBag.LocalListID = listId;
            var localList = db.LocalLists.Find(listId);
            ViewBag.Governorate = localList.Governorate;
            ViewBag.ElectionArea = localList.ElectionArea;

            int candidateCount = 0;

            if (localList.Governorate == "إربد" && localList.ElectionArea == "إربد الأولى")
            {
                candidateCount = 7;
            }
            else if (localList.Governorate == "إربد" && localList.ElectionArea == "إربد الثانية")
            {
                candidateCount = 6;
            }
            else if (localList.Governorate == "عجلون")
            {
                candidateCount = 3;
            }

            ViewBag.CandidateCount = candidateCount;
            Session["CandidateCount"] = candidateCount;
            PopulateDropDowns();
            return View(new List<LocalListCandidate>());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCandidates(List<LocalListCandidate> candidates)
        {
            if (candidates != null && candidates.Any())
            {
                if (TempData["LocalListID"] != null)
                {
                    int localListID = (int)TempData["LocalListID"];
                    foreach (var candidate in candidates)
                    {
                        candidate.LocalListingID = localListID;

                        var existingCandidate = db.LocalListCandidates
                            .FirstOrDefault(c => c.NationalNumber == candidate.NationalNumber && c.LocalListingID == localListID);

                        if (existingCandidate == null)
                        {
                            db.LocalListCandidates.Add(candidate);
                        }
                        else
                        {
                            ModelState.AddModelError("", "الرقم الوطني موجود بالفعل في القائمة.");
                            ViewBag.Governorate = TempData["Governorate"];
                            ViewBag.ElectionArea = TempData["ElectionArea"];
                            PopulateDropDowns();
                            return View(candidates);
                        }
                    }

                    try
                    {
                        db.SaveChanges();
                        return RedirectToAction("ThankYou");
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                                System.Diagnostics.Debug.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.Message);
                        System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                    }
                }
            }

            ViewBag.Governorate = TempData["Governorate"];
            ViewBag.ElectionArea = TempData["ElectionArea"];
            PopulateDropDowns();
            return View(candidates);
        }



        private void PopulateDropDowns(LocalListCandidate localListCandidate = null)
        {
            ViewBag.LocalListingID = new SelectList(db.LocalLists, "LocalListingID", "LocalListingName", localListCandidate?.LocalListingID);
            ViewBag.Religions = new SelectList(new List<string> { "مسلم", "مسيحي" }, localListCandidate?.Religion);
            ViewBag.Genders = new SelectList(new List<string> { "ذكر", "أنثى" }, localListCandidate?.Gender);
            ViewBag.Governorates = new SelectList(new List<string> { "إربد", "عجلون" }, localListCandidate?.Governorate);
            ViewBag.ElectionAreas = new SelectList(new List<string> { "إربد الأولى", "إربد الثانية", "عجلون" }, localListCandidate?.ElectionArea);
            ViewBag.TypeChairs = new SelectList(new List<string> { "كوتا", "مسيحي", "تنافس" }, localListCandidate?.Type_Chair);
        }


        public ActionResult ThankYou()
        {
            return View();
        }

        // GET: LocalListCandidates/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocalListCandidate localListCandidate = db.LocalListCandidates.Find(id);
            if (localListCandidate == null)
            {
                return HttpNotFound();
            }
            PopulateDropDowns(localListCandidate);
            return View(localListCandidate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CandidateID,CandidateName,NationalNumber,Gender,Religion,Type_Chair,BirthdateCandidate,Governorate,ElectionArea,NumberOfVotesCandidate,LocalListingID,Status")] LocalListCandidate localListCandidate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(localListCandidate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateDropDowns(localListCandidate);
            return View(localListCandidate);
        }

        // GET: LocalListCandidates/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LocalListCandidate localListCandidate = db.LocalListCandidates.Find(id);
            if (localListCandidate == null)
            {
                return HttpNotFound();
            }
            return View(localListCandidate);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            LocalListCandidate localListCandidate = db.LocalListCandidates.Find(id);
            db.LocalListCandidates.Remove(localListCandidate);
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
