using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext db = new MyDbContext();

        public ActionResult Index()
        {
            ViewBag.TotalEquipment = db.Equipments.Count();
            ViewBag.TotalUsers = db.Users.Count();
            ViewBag.TotalRentals = db.Rentals.Count();

            // When a user is logged in, also load their own items / rentals
            int? currentUserId = Session["UserID"] as int?;
            if (currentUserId != null)
            {
                ViewBag.MyEquipment = db.Equipments
                                         .Where(e => e.OwnerID == currentUserId)
                                         .ToList();

                ViewBag.MyRentals = db.Rentals
                                        .Where(r => r.RenterID == currentUserId)
                                        .Include(r => r.Equipment)
                                        .ToList();
            }

            return View();
        }

        public ActionResult About() => View();
        public ActionResult Contact() => View();
    }
}
