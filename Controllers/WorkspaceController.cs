using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class WorkspaceController : Controller
    {
        private readonly IProjectServices _projectServices;
        private readonly IUserServices _userServices;

        public WorkspaceController(
            IProjectServices projectServices,
            IUserServices userServices)
        {
            _projectServices = projectServices;
            _userServices = userServices;
        }

        // GET: /Workspace/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Project id không hợp lệ.");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            var currentUser = _userServices.GetUser(userEmail);           
            var projectEntity = await _projectServices.GetProjectEntityByIdAsync(id);          
            ViewBag.IsMentor = (currentUser?.RoleId == 1 && projectEntity?.CreatedBy == currentUser.UserId);
            var model = await _projectServices.GetWorkspaceAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewBag.ProjectId = id;
            return View(model);
        }
    }
}