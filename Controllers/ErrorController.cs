using Microsoft.AspNetCore.Mvc;

namespace PorjectManagement.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AccessDeny()
        {
            return View();
        }
    }
}
