using Microsoft.AspNetCore.Mvc;

namespace PorjectManagement.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDeny(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }
    }
}
