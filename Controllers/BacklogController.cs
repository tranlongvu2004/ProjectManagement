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
        public async Task<IActionResult> BacklogUI(int projectId)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            var tasks = await _context.Tasks
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            ViewBag.ProjectId = projectId;

            // ViewBag.Projects load tự động từ BaseController.OnActionExecuting
            // Nếu ko thích auto thì dùng cái dưới đức
            // int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            // ViewBag.Projects = await _projectServices.GetProjectsOfUserAsync(currentUserId);

            return View(tasks);
        }
    }
}