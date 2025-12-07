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
        public IActionResult BacklogUI()
        {
            var tasks = _context.Tasks.Include(t => t.CreatedByNavigation).ToList();
            return View(tasks);
        }
    }
}