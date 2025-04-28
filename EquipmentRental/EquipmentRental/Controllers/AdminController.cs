using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.UserCount = db.Users.Count();
            ViewBag.EquipmentCount = db.Equipments.Count();
            ViewBag.RentalCount = db.Rentals.Count();
            return View();
        }

        // GET: Admin/ManageRoles
        public ActionResult ManageRoles()
        {
            var users = db.Users.ToList();
            return View(users);
        }

        // POST: Admin/UpdateRole (non-AJAX)
        [HttpPost]
        public ActionResult UpdateRole(int userId, string role)
        {
            var user = db.Users.Find(userId);
            if (user == null) return HttpNotFound();

            user.Role = role;
            db.SaveChanges();

            return RedirectToAction("ManageRoles");
        }

        // POST: Admin/UpdateRoleAjax (AJAX approach)
        [HttpPost]
        public JsonResult UpdateRoleAjax(int userId, string role)
        {
            var user = db.Users.Find(userId);
            if (user == null)
                return Json(new { success = false, message = "User not found." });

            user.Role = role;
            db.SaveChanges();

            return Json(new { success = true, message = "Role updated." });
        }

        // GET: Admin/ApproveEquipment
        public ActionResult ApproveEquipment()
        {
            var unapproved = db.Equipments.Where(e => !e.IsApproved).ToList();
            return View(unapproved);
        }

        // POST: Admin/Approve (Single item approval)
        [HttpPost]
        public ActionResult Approve(int equipmentId)
        {
            var eq = db.Equipments.Find(equipmentId);
            if (eq == null) return HttpNotFound();

            eq.IsApproved = true;
            db.SaveChanges();

            return RedirectToAction("ApproveEquipment");
        }

        // list pending rental requests
        public ActionResult ManageRequests()
        {
            var pending = db.Rentals
                             .Where(r => r.Status == "Pending")
                             .Include(r => r.Equipment)
                             .Include(r => r.Renter)
                             .ToList();
            return View(pending);
        }

        // POST: Admin/DenyEquipment
        [HttpPost]
        public ActionResult DenyEquipment(int equipmentId)
        {
            var eq = db.Equipments.Find(equipmentId);
            if (eq == null) return HttpNotFound();

            db.Equipments.Remove(eq);
            db.SaveChanges();


            return RedirectToAction("ApproveEquipment");
        }

    }
}
