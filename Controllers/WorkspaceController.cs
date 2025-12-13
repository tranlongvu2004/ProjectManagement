using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

namespace PorjectManagement.Controllers
{
    public class WorkspaceController : BaseController
    {
        private readonly IProjectServices _projectServices;

        public WorkspaceController(IProjectServices projectServices)
        {
            _projectServices = projectServices;
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
            return View(model);
        }
    }
}