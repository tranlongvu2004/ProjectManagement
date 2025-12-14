using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

public class InternController : Controller
{
    private readonly IInternService _internService;

    public InternController(IInternService internService)
    {
        _internService = internService;
    }

    // GET: Intern
    public async Task<IActionResult> Index()
    {
        var interns = await _internService.GetInternsAsync();
        return View(interns);
    }
}
