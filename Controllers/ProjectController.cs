using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

public class ProjectController : Controller
{
    private readonly IProjectServices _projectServices;

    public ProjectController(IProjectServices projectServices)
    {
        _projectServices = projectServices;
    }

    public async Task<IActionResult> Index()
    {
        int currentUserId = HttpContext.Session.GetInt32("RoleId") ?? 0;

        var model = await _projectServices.GetProjectsOfUserAsync(currentUserId);

        return View(model);
    }
}
