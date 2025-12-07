using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IProjectServices _projectServices;

        public ProjectController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
        }

        // GET: /Project
        // Trang Project List: tạm thời lấy tất cả project
        public async Task<IActionResult> Index()
        {
            // TODO: sau này filter theo user đang đăng nhập
            List<Project> projects = await _projectServices.GetAllProjectsAsync();
            return View(projects);   // View: Views/Project/Index.cshtml
        }

        // GET: /Project/Workspace/5
        // Trang chi tiết Workspace của 1 project
        public async Task<IActionResult> Workspace(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Project id không hợp lệ.");
            }

            ProjectWorkspaceViewModel? model = await _projectServices.GetWorkspaceAsync(id);
            if (model == null)
            {
                return NotFound(); // Không tìm thấy project
            }

            return View(model);     // View: Views/Project/Workspace.cshtml
        }
    }
}
