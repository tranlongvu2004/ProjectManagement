using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ProjectManagementController : Controller
    {
        private readonly IProjectServices _projectServices;
        private readonly IUserServices _userServices;        
        public ProjectManagementController(
            IProjectServices projectServices,
            IUserServices userServices)
        {
            _projectServices = projectServices;
            _userServices = userServices;
        }

        // GET: /ProjectManagement/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if(userId == 0)
            {
                return RedirectToAction("Login", "User");
            }
            if (roleId != 1)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("<script>alert('Login information not found.'); window.location.href='/Project/Index';</script>", "text/html");
            }
            var currentUser = _userServices.GetUser(userEmail);

            if (currentUser == null || currentUser.RoleId != 1)
            {
                return Content("<script>alert('Only mentors can create projects..'); window.location.href='/Project/Index';</script>", "text/html");
            }

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddMonths(1),
                AvailableUsers = await _projectServices.GetAvailableUsersAsync()
            };

            return View(model);
        }

        // POST: /ProjectManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("<script>alert('Login information not found.'); window.location.href='/Project/Index';</script>", "text/html");
            }
            var currentUser = _userServices.GetUser(userEmail);

            if (currentUser == null || currentUser.RoleId != 1)
            {
                return Content("<script>alert('No permission to create project.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            if (model.Deadline.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Deadline", "Deadline not in the past");
            }
            
            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Please select at least 1 member for project.");
            }

            if (!model.LeaderId.HasValue || model.LeaderId.Value <= 0)
            {
                ModelState.AddModelError("LeaderId", "Please select Leader for project.");
            }

            else if (model.SelectedUserIds != null && !model.SelectedUserIds.Contains(model.LeaderId.Value))
            {
                ModelState.AddModelError("LeaderId", "Leader must be member of project.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }

            try
            {
                int projectId = await _projectServices.CreateProjectWithTeamAsync(model, currentUser.UserId);
               
                return Content($"<script>alert('Create project sucessfully!'); window.location.href='/Workspace/Details/{projectId}';</script>", "text/html");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error when create project: {ex.Message}");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                return View(model);
            }
        }

        // GET: /ProjectManagement/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 1)
            {
                return RedirectToAction("AccessDeny", "Error");
            }
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("<script>alert('Login information not found.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            var currentUser = _userServices.GetUser(userEmail);
            if (currentUser == null || currentUser.RoleId != 1)
            {
                return Content("<script>alert('Only Mentor can update project.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            var model = await _projectServices.GetProjectForUpdateAsync(id, currentUser.UserId);
            
            if (model == null)
            {
                return Content("<script>alert('Project not found or you don't have editing permissions.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            return View(model);
        }

        // POST: /ProjectManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectUpdateViewModel model)
        {

            if (id != model.ProjectId)
            {
                return Content("<script>alert('Data not valid.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Content("<script>alert('Login information not found.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            var currentUser = _userServices.GetUser(userEmail);
            if (currentUser == null || currentUser.RoleId != 1)
            {
                return Content("<script>alert('No update permissions project.'); window.location.href='/Project/Index';</script>", "text/html");
            }

            if (model.Deadline.Date < DateTime.Now.Date)
            {
                ModelState.AddModelError("Deadline", "Deadline not in past");
            }

            if (model.SelectedUserIds == null || !model.SelectedUserIds.Any())
            {
                ModelState.AddModelError("SelectedUserIds", "Please select at least 1 member for project.");
            }

            if (!model.LeaderId.HasValue || model.LeaderId.Value <= 0)
            {
                ModelState.AddModelError("LeaderId", "Please select Leader for project.");
            }
            else if (model.SelectedUserIds != null && !model.SelectedUserIds.Contains(model.LeaderId.Value))
            {
                ModelState.AddModelError("LeaderId", "Leader must be member of project.");
            }

            if (!ModelState.IsValid)
            {
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                model.CurrentMembers = await _projectServices.GetProjectMembersAsync(id);
                return View(model);
            }

            try
            {
                bool success = await _projectServices.UpdateProjectWithTeamAsync(model, currentUser.UserId);
                
                if (!success)
                {
                    return Content("<script>alert('Cant update project.'); window.location.href='/Project/Index';</script>", "text/html");
                }

                return Content($"<script>alert('Update project sucessfully!'); window.location.href='/Workspace/Details/{model.ProjectId}';</script>", "text/html");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error when update project: {ex.Message}");
                model.AvailableUsers = await _projectServices.GetAvailableUsersAsync();
                model.CurrentMembers = await _projectServices.GetProjectMembersAsync(id);
                return View(model);
            }
        }
    }
}