using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class ProjectController : BaseController
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }

        public async Task<IActionResult> Index()
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

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
