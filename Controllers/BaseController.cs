using Microsoft.AspNetCore.Mvc;

namespace PorjectManagement.Controllers
{
    public class BaseController : Controller
    {
        protected bool IsLoggedIn()
        {
            return HttpContext.Session.GetString("UserEmail") != null;
        }

        protected IActionResult RedirectIfNotLoggedIn()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "User");

            return null!;
        }
    }
}
