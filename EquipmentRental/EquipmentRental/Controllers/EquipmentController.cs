using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    public class EquipmentController : Controller
    {
        private readonly MyDbContext db = new MyDbContext();


        private static readonly string[] CategoryList =
        {
            "Tools", "Electronics", "Heavy Machinery", "Garden", "Other"
        };

        /* ----------  INDEX with search ---------- */
        [AllowAnonymous]
        public ActionResult Index(string q = null)
        {
            var list = db.Equipments
                         .Where(e => e.IsApproved &&
                                     (q == null ||
                                      e.Name.Contains(q) ||
                                      e.Category.Contains(q)))
                         .Include(e => e.Owner)
                         .OrderBy(e => e.Name)
                         .ToList();


            if (Request.IsAjaxRequest())
            {
                return PartialView("_EquipmentCardGrid", list);
            }

            // Otherwise, return full page load
            return View(list);
        }

        /* ----------  Autocomplete JSON ---------- */
        [HttpGet]
        [AllowAnonymous]
        public JsonResult AutoComplete(string term)
        {
            var suggestions = db.Equipments
                                .Where(e => e.IsApproved &&
                                            (e.Name.Contains(term) || e.Category.Contains(term)))
                                .Select(e => e.Name)
                                .Distinct()
                                .Take(10)
                                .ToList();
            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        /* ----------  DETAILS  ---------- */
        [AllowAnonymous]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);


            var eq = db.Equipments
                       .Include(e => e.Owner)
                       .FirstOrDefault(e => e.ID == id.Value);

            if (eq == null)
                return HttpNotFound();


            var approvedBookings = db.Rentals
                                     .Where(r => r.EquipmentID == eq.ID && r.Status == "Approved")
                                     .OrderBy(r => r.RentalStartDate)
                                     .ToList();
            ViewBag.Bookings = approvedBookings;

            return View(eq);
        }

        /*********************  CREATE  *************************/
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.Categories = new SelectList(CategoryList);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "Name,Category,RentalPrice,Description")] Equipment equipment,
                                   HttpPostedFileBase EquipmentImage)
        {
            ViewBag.Categories = new SelectList(CategoryList, equipment.Category);
            if (!ModelState.IsValid) return View(equipment);

            int? uid = Session["UserID"] as int?;
            if (uid == null) return new HttpStatusCodeResult(403);

            equipment.OwnerID = uid.Value;
            equipment.IsApproved = false;
            equipment.AvailabilityStatus = "Available";

            HandleImageUpload(equipment, EquipmentImage);

            db.Equipments.Add(equipment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /*********************  EDIT  *************************/
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var eq = db.Equipments.Find(id);
            if (eq == null) return HttpNotFound();

            if (!CanEditOrDelete(eq)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            ViewBag.Categories = new SelectList(CategoryList, eq.Category);
            return View(eq);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Edit([Bind(Include = "ID,Name,Category,RentalPrice,Description")] Equipment equipment,
                                 HttpPostedFileBase EquipmentImage)
        {
            ViewBag.Categories = new SelectList(CategoryList, equipment.Category);
            if (!ModelState.IsValid) return View(equipment);

            var original = db.Equipments.Find(equipment.ID);
            if (original == null) return HttpNotFound();
            if (!CanEditOrDelete(original)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            original.Name = equipment.Name;
            original.Category = equipment.Category;
            original.RentalPrice = equipment.RentalPrice;
            original.Description = equipment.Description;


            HandleImageUpload(original, EquipmentImage);

            db.Entry(original).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /*********************  DELETE  *************************/
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var eq = db.Equipments.Find(id);
            if (eq == null) return HttpNotFound();
            if (!CanEditOrDelete(eq)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(eq);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult DeleteConfirmed(int id)
        {
            var eq = db.Equipments.Find(id);
            if (eq == null) return HttpNotFound();
            if (!CanEditOrDelete(eq)) return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            db.Equipments.Remove(eq);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        /*********************  HELPERS  *************************/
        private bool CanEditOrDelete(Equipment eq)
        {
            int? uid = Session["UserID"] as int?;
            return User.IsInRole("Admin") || (uid != null && uid == eq.OwnerID);
        }

        private void HandleImageUpload(Equipment eq, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return;
            var uploadPath = Server.MapPath("~/Uploads");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            file.SaveAs(Path.Combine(uploadPath, fileName));
            eq.ImagePath = "/Uploads/" + fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
