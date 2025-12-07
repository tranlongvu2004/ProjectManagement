using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class WorkspaceController : BaseController
    {
        private readonly IProjectServices _projectServices;
        private readonly LabProjectManagementContext _context;

        public WorkspaceController(IProjectServices projectServices, LabProjectManagementContext context)
        {
            _projectServices = projectServices;
            _context = context;
        }

        // GET: /Workspace/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var redirect = RedirectIfNotLoggedIn();
            if (redirect != null) return redirect;

            if (id <= 0)
            {
                return BadRequest("Project id không hợp lệ.");
            }

            // Lấy workspace data
            var model = await _projectServices.GetWorkspaceAsync(id);
            if (model == null)
            {
                return NotFound();
            }

            // Lấy danh sách projects cho sidebar
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var projects = await _projectServices.GetProjectsOfUserAsync(currentUserId);
            ViewBag.Projects = projects;

            // ✅ Lấy tasks cho Backlog
            var tasks = await _context.Tasks
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ProjectId == id)
                .ToListAsync();
            ViewBag.BacklogTasks = tasks;
            ViewBag.ProjectId = id;

            return View(model);
        }
    }
}