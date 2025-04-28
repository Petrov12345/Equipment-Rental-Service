using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    public class RentalController : Controller
    {
        private readonly MyDbContext db = new MyDbContext();

        /* ----------  INDEX  ---------- */
        [Authorize]
        public ActionResult Index(string q = null)
        {
            //Always show only the current user's rentals
            int uid = (int)Session["UserID"];
            IQueryable<Rental> query = db.Rentals
                                         .Include(r => r.Equipment)
                                         .Where(r => r.RenterID == uid);

            //Optionally filter by search
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(r => r.Equipment.Name.Contains(q));
            }

            var list = query.OrderByDescending(r => r.RentalStartDate).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_UserRentalCards", list);

            return View(list); 
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AdminIndex(string q = null)
        {
            // Admin can see all rentals
            IQueryable<Rental> query = db.Rentals
                                         .Include(r => r.Equipment.Owner)
                                         .Include(r => r.Renter);

            if (!string.IsNullOrWhiteSpace(q))
            {
                // Search by Equipment name, Equipment Owner name, or Renter name
                query = query.Where(r =>
                    r.Equipment.Name.Contains(q) ||
                    r.Equipment.Owner.Name.Contains(q) ||
                    r.Renter.Name.Contains(q));
            }

            var list = query.OrderByDescending(r => r.RentalStartDate).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_RentalTable", list);

            return View("AdminIndex", list);
        }


        /* ----------  autocomplete ---------- */
        [HttpGet, Authorize]
        public JsonResult AutoComplete(string term)
        {
            var results = db.Rentals
                            .Where(r => r.Equipment.Name.Contains(term))
                            .Select(r => r.Equipment.Name)
                            .Distinct()
                            .Take(10)
                            .ToList();
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        /* ----------  DETAILS  ---------- */
        [Authorize]
        public ActionResult Details(int id)
        {
            var r = db.Rentals.Include(x => x.Equipment.Owner)
                              .Include(x => x.Renter)
                              .FirstOrDefault(x => x.ID == id);
            if (r == null) return HttpNotFound();
            if (!User.IsInRole("Admin") && (Session["UserID"] as int?) != r.RenterID)
                return new HttpStatusCodeResult(403);
            return View(r);
        }

        /* ----------  CREATE ---------- */
        [Authorize]
        public ActionResult Create(int? equipmentId = null)
        {
            if (equipmentId == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var equip = db.Equipments.Find(equipmentId);
            if (equip == null || !equip.IsApproved) return HttpNotFound();

            ViewBag.Equipment = equip;
            return View(new Rental { EquipmentID = equip.ID });
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult Create([Bind(Include = "EquipmentID,RentalStartDate,RentalEndDate")] Rental rental)
        {
            var equip = db.Equipments
                          .Include(e => e.Owner)
                          .FirstOrDefault(e => e.ID == rental.EquipmentID);
            if (equip == null || !equip.IsApproved) return HttpNotFound();

            int uid = (int)Session["UserID"];
            if (rental.RentalStartDate >= rental.RentalEndDate)
                ModelState.AddModelError("", "End date must be after start date.");

            // CHANGE #1: More descriptive error when date overlap occurs
            var overlappingRentals = db.Rentals
                .Where(r => r.EquipmentID == equip.ID && r.Status == "Approved"
                         && r.RentalEndDate >= rental.RentalStartDate
                         && r.RentalStartDate <= rental.RentalEndDate)
                .ToList();

            if (overlappingRentals.Any())
            {
                var conflictRanges = overlappingRentals.Select(r =>
                    $"{r.RentalStartDate:MM/dd/yyyy} - {r.RentalEndDate:MM/dd/yyyy}");
                string conflictMessage = "Cannot book. Equipment is already rented on these dates: "
                                         + string.Join("; ", conflictRanges);
                ModelState.AddModelError("", conflictMessage);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Equipment = equip;
                return View(rental);
            }

            int days = (rental.RentalEndDate - rental.RentalStartDate).Days + 1;
            rental.TotalCost = days * equip.RentalPrice;
            rental.RenterID = uid;
            rental.Status = "Pending";
            rental.ApproverID = null;

            db.Rentals.Add(rental);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /* ----------  EDIT  ---------- */
        [Authorize]
        public ActionResult Edit(int id)
        {
            var rental = db.Rentals.Include(r => r.Equipment.Owner)
                                   .Include(r => r.Renter)
                                   .FirstOrDefault(r => r.ID == id);
            if (rental == null) return HttpNotFound();
            bool isOwner = (Session["UserID"] as int?) == rental.RenterID;

            if (rental.Status != "Pending" || (!isOwner && !User.IsInRole("Admin")))
                return new HttpStatusCodeResult(403);

            return View(rental);
        }

        [HttpPost, ValidateAntiForgeryToken, Authorize]
        public ActionResult Edit([Bind(Include = "ID,RentalStartDate,RentalEndDate")] Rental edited)
        {
            var rental = db.Rentals.Include(r => r.Equipment)
                                   .FirstOrDefault(r => r.ID == edited.ID);
            if (rental == null) return HttpNotFound();
            bool isOwner = (Session["UserID"] as int?) == rental.RenterID;

            if (rental.Status != "Pending" || (!isOwner && !User.IsInRole("Admin")))
                return new HttpStatusCodeResult(403);

            if (edited.RentalStartDate >= edited.RentalEndDate)
                ModelState.AddModelError("", "End date must be after start date.");

            // error when date overlap occurs
            var overlappingRentals = db.Rentals
                .Where(r => r.ID != rental.ID
                         && r.EquipmentID == rental.EquipmentID
                         && r.Status == "Approved"
                         && r.RentalEndDate >= edited.RentalStartDate
                         && r.RentalStartDate <= edited.RentalEndDate)
                .ToList();

            if (overlappingRentals.Any())
            {
                var conflictRanges = overlappingRentals.Select(r =>
                    $"{r.RentalStartDate:MM/dd/yyyy} - {r.RentalEndDate:MM/dd/yyyy}");
                string conflictMessage = "Cannot edit. Equipment is already rented on these dates: "
                                         + string.Join("; ", conflictRanges);
                ModelState.AddModelError("", conflictMessage);
            }

            if (!ModelState.IsValid) return View(rental);

            rental.RentalStartDate = edited.RentalStartDate;
            rental.RentalEndDate = edited.RentalEndDate;
            int days = (edited.RentalEndDate - edited.RentalStartDate).Days + 1;
            rental.TotalCost = days * rental.Equipment.RentalPrice;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /* ----------  DELETE ---------- */
        [Authorize]
        public ActionResult Delete(int id)
        {
            var rental = db.Rentals.Include(r => r.Equipment.Owner)
                                   .Include(r => r.Renter)
                                   .FirstOrDefault(r => r.ID == id);
            if (rental == null) return HttpNotFound();

            bool isOwner = (Session["UserID"] as int?) == rental.RenterID;
            if (!isOwner && !User.IsInRole("Admin"))
                return new HttpStatusCodeResult(403);

            return View(rental);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken, Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            var rental = db.Rentals.Find(id);
            if (rental == null) return HttpNotFound();

            bool isOwner = (Session["UserID"] as int?) == rental.RenterID;
            if (!isOwner && !User.IsInRole("Admin"))
                return new HttpStatusCodeResult(403);

            db.Rentals.Remove(rental);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
