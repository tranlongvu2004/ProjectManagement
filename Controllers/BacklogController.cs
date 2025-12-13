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
        private readonly IUserProjectService _up;

        public BacklogController(
            LabProjectManagementContext context,
            IProjectServices projectServices,
            IUserProjectService up)
        {
            _context = context;
            _projectServices = projectServices;
            _up = up;
        }
        public IActionResult BacklogUI(int projectId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }
            bool isLeader = _up.IsleaderOfProject(userId.Value, projectId);
            // Theo dev/Vu
            var tasks = _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Where(t => t.ProjectId == projectId)
                .ToList();

            ViewBag.ProjectId = projectId;
            ViewBag.IsLeader = isLeader;
            return View(tasks);
        }
    }
}