using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Controllers
{
    public class BacklogController : Controller
    {
        private LabProjectManagementContext _context;
        public BacklogController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public IActionResult BacklogUI(int projectId)
        {
            
            var tasks = _context.Tasks
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ProjectId == projectId)
                .ToList();           
            ViewBag.ProjectId = projectId;
            return View(tasks);
        }
    }
}