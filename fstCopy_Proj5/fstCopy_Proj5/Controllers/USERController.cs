﻿using fstCopy_Proj5.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace fstCopy_Proj5.Controllers
{
    public class USERController : Controller
    {
        private ElectionEntities1 DB = new ElectionEntities1();

        // GET: USER
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(USER user)
        {
            try
            {
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View();
                }

                var existingUser = DB.USERS.FirstOrDefault(u => u.National_ID == user.National_ID);

                if (existingUser == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View();
                }

                if (existingUser.Password == "password")
                {
                    string newPassword = GenerateRandomPassword();
                    existingUser.Password = newPassword;
                    DB.SaveChanges();

                    SendConfirmationEmail(existingUser.Email, newPassword);

                    ViewBag.Emailsent = "The code has been sent to your Email";
                }

                // تخزين المستخدم في الجلسة وإعادة التوجيه إلى LoginUser
                Session["LoggedUser"] = JsonConvert.SerializeObject(existingUser);
                return RedirectToAction("LoginUser", new { ID = user.National_ID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                Console.WriteLine("Exception message: " + ex.Message);
            }

            return View();
        }

        public ActionResult LoginUser(int nationalNumber)
        {
            var user = DB.USERS.FirstOrDefault(u => u.National_ID == nationalNumber);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View();
            }

            // Save in session
            Session["LoggedUser"] = JsonConvert.SerializeObject(user);
            ViewBag.NationalNumber = nationalNumber;
            ViewBag.Email = user.Email;

            // Redirect to the new page with buttons
            return RedirectToAction("ElectionDistricts");
        }


        //Generate password
        private string GenerateRandomPassword()
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            int length = 8;
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        // Send Email
        private void SendConfirmationEmail(string toEmail, string confirmationCode)
        {
            string fromEmail = System.Configuration.ConfigurationManager.AppSettings["FromEmail"];
            string smtpUsername = System.Configuration.ConfigurationManager.AppSettings["SmtpUsername"];
            string smtpPassword = System.Configuration.ConfigurationManager.AppSettings["SmtpPassword"];

            string subjectText = "Your Confirmation Code";
            string messageText = $"Your confirmation code is {confirmationCode}";

            string smtpServer = "smtp.gmail.com";
            int smtpPort = 587;

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(fromEmail);
                mailMessage.To.Add(toEmail);
                mailMessage.Subject = subjectText;
                mailMessage.Body = messageText;
                mailMessage.IsBodyHtml = false;

                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;

                    smtpClient.Send(mailMessage);
                }
            }
        }

        public ActionResult TypeOfElection(int id)
        {
            var user = DB.USERS.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Save the user to the session
            Session["LoggedUser"] = JsonConvert.SerializeObject(user);
            ViewBag.NationalNumber = user.National_ID;

            // Determine the paths for elections
            ViewBag.LocalElectionsPath = Convert.ToBoolean( user.local_Vote) ? null : "LocalElections";
            ViewBag.PartyElectionsPath = Convert.ToBoolean( user.Party_Vote) ? null : "PartyElections";

            return View();
        }



        public ActionResult LocalElections()
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var userJson = Session["LoggedUser"].ToString();
            var user = JsonConvert.DeserializeObject<USER>(userJson);

            ViewBag.UserId = user.National_ID;

            var localLists = DB.LocalLists.Where(l => l.ElectionArea == user.ElectionArea).ToList();
            var candidates = DB.LocalListCandidates.ToList();

            ViewBag.LocalLists = localLists;
            ViewBag.Candidates = candidates;

            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LocalElections(int selectedListId, int[] selectedCandidateIds)
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var userJson = Session["LoggedUser"].ToString();
            var user = JsonConvert.DeserializeObject<USER>(userJson);

            var selectedList = DB.LocalLists.Find(selectedListId);

            if (selectedList != null && selectedCandidateIds != null)
            {
                // Update the number of votes for the selected list
                selectedList.NumberOfVotes += 1;
                DB.Entry(selectedList).State = System.Data.Entity.EntityState.Modified;

                // Update the number of votes for the selected candidates
                foreach (var candidateId in selectedCandidateIds)
                {
                    var selectedCandidate = DB.LocalListCandidates.FirstOrDefault(c => c.CandidateID == candidateId && c.LocalListingID == selectedListId);
                    if (selectedCandidate != null)
                    {
                        selectedCandidate.NumberOfVotesCandidate += 1;
                        DB.Entry(selectedCandidate).State = System.Data.Entity.EntityState.Modified;
                    }
                }

                // Update user table to reflect that the user has voted in local elections
                user.local_Vote = 1;
                DB.Entry(user).State = System.Data.Entity.EntityState.Modified;

                DB.SaveChanges();
            }

            return RedirectToAction("TypeOfElection", new { id = user.National_ID });
        }


        public ActionResult PartyElections()
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var userJson = Session["LoggedUser"].ToString();
            var user = JsonConvert.DeserializeObject<USER>(userJson);

            ViewBag.UserId = user.National_ID;

            // Retrieve all party lists to display in the view
            var partyLists = DB.GeneralListings.ToList();
            return View(partyLists);
        }

        [HttpPost]
        public ActionResult PartyElections(int selectedPartyListId)
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var userJson = Session["LoggedUser"].ToString();
            var user = JsonConvert.DeserializeObject<USER>(userJson);

            var selectedPartyList = DB.GeneralListings.Find(selectedPartyListId);
            if (selectedPartyList != null)
            {
                // Update user table to reflect that the user has voted in party elections
                user.Party_Vote = 1;
                user.local_Vote = 1; // Optionally, you can set LocalElections to true here if needed

                // Update the party list to increment the number of votes
                selectedPartyList.NumberOfVotes += 1;
                DB.Entry(selectedPartyList).State = System.Data.Entity.EntityState.Modified;
                DB.Entry(user).State = System.Data.Entity.EntityState.Modified; // Ensure user entity is being tracked
                DB.SaveChanges();
            }

            return RedirectToAction("TypeOfElection", new { id = user.National_ID });
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///
        public ActionResult ElectionDistricts()
        {
            if (Session["LoggedUser"] == null)
            {
                return RedirectToAction("Login");
            }

            var userJson = Session["LoggedUser"].ToString();
            var user = JsonConvert.DeserializeObject<USER>(userJson);

            ViewBag.ElectionArea = user.ElectionArea;

            return View();
        }


        public ActionResult IrbedFirstDistrict()
        {
            try
            {
                if (Session["LoggedUser"] == null)
                {
                    return RedirectToAction("Login");
                }

                var userJson = Session["LoggedUser"].ToString();
                var user = JsonConvert.DeserializeObject<USER>(userJson);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return RedirectToAction("Login");
                }

                user.ElectionArea = "Area 1"; // Setting election area to IrbedFirstDistrict
                DB.Entry(user).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();

                // Redirect to TypeOfElection with the user's ID
                return RedirectToAction("TypeOfElection", new { id = user.National_ID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                Console.WriteLine("Exception message: " + ex.Message);
                return RedirectToAction("Login");
            }
        }


        public ActionResult IrbedSecondDistrict()
        {
            try
            {
                if (Session["LoggedUser"] == null)
                {
                    return RedirectToAction("Login");
                }

                var userJson = Session["LoggedUser"].ToString();
                var user = JsonConvert.DeserializeObject<USER>(userJson);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return RedirectToAction("Login");
                }

                user.ElectionArea = "Area 2"; // Setting election area to IrbedFirstDistrict
                DB.Entry(user).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();

                // Redirect to TypeOfElection with the user's ID
                return RedirectToAction("TypeOfElection", new { id = user.National_ID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                Console.WriteLine("Exception message: " + ex.Message);
                return RedirectToAction("Login");
            }
        }

        public ActionResult AjlounDistrict()
        {
            try
            {
                if (Session["LoggedUser"] == null)
                {
                    return RedirectToAction("Login");
                }

                var userJson = Session["LoggedUser"].ToString();
                var user = JsonConvert.DeserializeObject<USER>(userJson);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return RedirectToAction("Login");
                }

                user.ElectionArea = "Area 3"; // Setting election area to IrbedFirstDistrict
                DB.Entry(user).State = System.Data.Entity.EntityState.Modified;
                DB.SaveChanges();

                // Redirect to TypeOfElection with the user's ID
                return RedirectToAction("TypeOfElection", new { id = user.National_ID });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                Console.WriteLine("Exception message: " + ex.Message);
                return RedirectToAction("Login");
            }
        }
    }

}