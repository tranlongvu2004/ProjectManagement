using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class BacklogController : BaseController
    {
        private readonly LabProjectManagementContext _context;
        private readonly IProjectServices _projectServices;

        public BacklogController(
            LabProjectManagementContext context,
            IProjectServices projectServices)
        {
            _context = context;
            _projectServices = projectServices;
        }
        public IActionResult BacklogUI(int projectId)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            // Theo dev/Vu
            var tasks = _context.Tasks
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ProjectId == projectId)
                .ToList();

            ViewBag.ProjectId = projectId;

            // ViewBag.Projects tự động load từ BaseController.OnActionExecuting

            return View(tasks);
        }
    }
}