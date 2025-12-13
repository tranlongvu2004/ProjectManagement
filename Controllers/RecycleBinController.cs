using Microsoft.AspNetCore.Mvc;

namespace PorjectManagement.Controllers
{
    public class RecycleBinController : Controller
    {
        public IActionResult RecycleBin()
        {
            return View();
        }
    }
}
