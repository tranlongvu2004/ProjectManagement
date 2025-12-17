using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }

        // GET: /Project
        public async Task<IActionResult> Index()
        {

            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "User");
            }
            ViewBag.RoleId = roleId;
            var model = await _projectServices.GetProjectsOfUserAsync(currentUserId);
            return View(model);
        }
    }
}
