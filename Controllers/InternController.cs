using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;

public class InternController : Controller
{
    private readonly IInternService _internService;

    public InternController(IInternService internService)
    {
        _internService = internService;
    }

    public async Task<IActionResult> Index(string? keyword,
                                           string sortBy = "name",
                                           string sortDir = "asc",
                                           int page = 1)
    {
        int pageSize = 7;
        int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
        if (roleId != 1)
        {
            return RedirectToAction("AccessDeny", "Error");
        }
        var query = await _internService.GetInternsAsync();

        // Search by name
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(u => u.FullName.Contains(keyword));
        }

        // Sort
        query = (sortBy, sortDir) switch
        {
            ("created", "asc") => query.OrderBy(u => u.CreatedAt),
            ("created", "desc") => query.OrderByDescending(u => u.CreatedAt),
            ("name", "desc") => query.OrderByDescending(u => u.FullName),
            _ => query.OrderBy(u => u.FullName)
        };

        int totalItems = query.Count();

        var interns = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new InternListVM
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                CreatedAt = u.CreatedAt
            })
            .ToList();

        var vm = new InternFilterVM
        {
            Keyword = keyword,
            SortBy = sortBy,
            SortDir = sortDir,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Interns = interns
        };

        return View(vm);
    }

}
