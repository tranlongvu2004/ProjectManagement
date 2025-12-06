using Microsoft.AspNetCore.Mvc;

namespace PorjectManagement.Controllers
{
    public class BacklogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
