// Controllers/UserController.cs  (fixed HashPassword calls)
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using EquipmentRental.Models;

namespace EquipmentRental.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDbContext db = new MyDbContext();

        /*********************  AUTHENTICATION  *************************/
        [AllowAnonymous]
        public ActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Please enter both email and password.");
                return View();
            }

            string hashed = EquipmentRental.Models.User.HashPassword(password);
            var user = db.Users.FirstOrDefault(u => u.Email == email && u.Password == hashed);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            FormsAuthentication.SetAuthCookie(user.Email, false);
            Session["UserID"] = user.ID;
            Session["UserName"] = user.Name;
            Session["UserRole"] = user.Role;
            return RedirectToAction("Index", "Home");
        }

        /*********************  REGISTRATION  *************************/
        [AllowAnonymous]
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Register(EquipmentRental.Models.User newUser)
        {
            if (!ModelState.IsValid) return View(newUser);

            if (db.Users.Any(u => u.Email == newUser.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(newUser);
            }

            newUser.Password = EquipmentRental.Models.User.HashPassword(newUser.Password);
            newUser.Role = "User";

            db.Users.Add(newUser);
            db.SaveChanges();

            FormsAuthentication.SetAuthCookie(newUser.Email, false);
            Session["UserID"] = newUser.ID;
            Session["UserName"] = newUser.Name;
            Session["UserRole"] = newUser.Role;
            return RedirectToAction("Index", "Home");
        }

        /*********************  LOGOUT  *************************/
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        /*********************  ADMIN CRUD  *************************/
        /* ----------  INDEX with search ---------- */
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string q = null)
        {
            var list = db.Users
                         .Where(u => q == null ||
                                     u.Name.Contains(q) ||
                                     u.Email.Contains(q))
                         .OrderBy(u => u.Name)
                         .ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_UserTable", list);

            return View(list);
        }

        /* ----------  Autocomplete ---------- */
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public JsonResult AutoComplete(string term)
        {
            var names = db.Users.Where(u => u.Name.Contains(term))
                                .Select(u => u.Name)
                                .Distinct()
                                .Take(10)
                                .ToList();
            return Json(names, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Details(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(EquipmentRental.Models.User edited)
        {
            if (!ModelState.IsValid) return View(edited);

            var original = db.Users.Find(edited.ID);
            if (original == null) return HttpNotFound();

            original.Name = edited.Name;
            original.Email = edited.Email;
            original.Address = edited.Address;
            original.Telephone = edited.Telephone;
            original.Role = edited.Role;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return HttpNotFound();

            db.Users.Remove(user);
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
