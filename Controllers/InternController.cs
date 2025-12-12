using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

public class InternController : Controller
{
    private readonly LabProjectManagementContext _context;

    public InternController(LabProjectManagementContext context)
    {
        _context = context;
    }

    // GET: Intern
    public async Task<IActionResult> Index()
    {
        var interns = await _context.Users
                          .Where(u => u.RoleId == 2)   
                          .ToListAsync();

        return View(interns);
    }
}
