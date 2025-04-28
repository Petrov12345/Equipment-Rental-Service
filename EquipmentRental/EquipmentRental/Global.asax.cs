using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using EquipmentRental.Models; 

namespace EquipmentRental
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {

            HttpCookie authCookie = HttpContext.Current?.Request?.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return;

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {

                return;
            }

            if (ticket == null) return;


            string userEmail = ticket.Name;
            if (string.IsNullOrEmpty(userEmail))
                return;


            using (var db = new MyDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                    return; 


                string[] roles = string.IsNullOrEmpty(user.Role)
                                     ? new string[] { }
                                     : new string[] { user.Role };


                var identity = new GenericIdentity(userEmail);
                var principal = new System.Security.Principal.GenericPrincipal(identity, roles);


                HttpContext.Current.User = principal;
            }
        }
    }
}
