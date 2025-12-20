using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class ProjectManagementControllerTests
    {
        private readonly Mock<IProjectServices> _projectServicesMock;
        private readonly Mock<IUserServices> _userServicesMock;
        private readonly ProjectManagementController _controller;
        private readonly DefaultHttpContext _httpContext;

        public ProjectManagementControllerTests()
        {
            _projectServicesMock = new Mock<IProjectServices>();
            _userServicesMock = new Mock<IUserServices>();

            _controller = new ProjectManagementController(
                _projectServicesMock.Object,
                _userServicesMock.Object
            );

            _httpContext = new DefaultHttpContext();
            _httpContext.Session = new TestSession();

            // Default: logged in as Mentor
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = _httpContext
            };

            _controller.TempData = new TempDataDictionary(
                _httpContext,
                Mock.Of<ITempDataProvider>()
            );
        }

        // ============================================================
        // CREATE - GET
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_NotLoggedIn_RedirectsToLogin()
        {
            _httpContext.Session.Clear();

            var result = await _controller.Create();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_NotMentor_RedirectsToAccessDeny()
        {
            _httpContext.Session.SetInt32("UserId", 2);
            _httpContext.Session.SetInt32("RoleId", 2);
            _httpContext.Session.SetString("UserEmail", "intern@example.com");

            var result = await _controller.Create();

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_NoUserEmail_ReturnsContentWithLoginInfoNotFound()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.Remove("UserEmail");

            var result = await _controller.Create();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Login information not found.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_CurrentUserNull_ReturnsContentOnlyMentorCanCreate()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns((User?)null);

            var result = await _controller.Create();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Only mentors can create projects", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_ValidMentor_ReturnsViewWithModel()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1, Email = "mentor@example.com" });

            var availableUsers = new List<AvailableUserItem>
            {
                new AvailableUserItem { UserId = 2, FullName = "Intern 1", Email = "intern1@example.com" }
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(availableUsers);

            var result = await _controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectCreateViewModel>(viewResult.Model);

            Assert.NotNull(model.AvailableUsers);
            Assert.Single(model.AvailableUsers);
            Assert.True(model.Deadline > DateTime.Now);
        }

        // ============================================================
        // CREATE - POST
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NotLoggedIn_ReturnsContent_LoginInfoNotFound()
        {
            // Controller POST does NOT check UserId; it checks UserEmail.
            _httpContext.Session.Clear();

            var model = new ProjectCreateViewModel();

            var result = await _controller.Create(model);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Login information not found.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_UserNotMentor_ReturnsContent_NoPermission()
        {
            _httpContext.Session.SetInt32("UserId", 2);
            _httpContext.Session.SetInt32("RoleId", 2);
            _httpContext.Session.SetString("UserEmail", "intern@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("intern@example.com"))
                .Returns(new User { UserId = 2, RoleId = 2, Email = "intern@example.com" });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 3 },
                LeaderId = 3
            };

            var result = await _controller.Create(model);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("No permission to create project.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_DeadlineInPast_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(-1),
                SelectedUserIds = new List<int> { 2 },
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NoMembers_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = null,
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("SelectedUserIds"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NoLeader_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 },
                LeaderId = null
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("LeaderId"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_LeaderNotInMembers_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 },
                LeaderId = 4
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("LeaderId"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_LeaderIsMentor_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 },
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("LeaderId"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_ValidModel_ReturnsContent_SuccessRedirectWorkspace()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                ProjectName = "New Project",
                Description = "Test Description",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 2, 3, 1 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.CreateProjectWithTeamAsync(model, 1))
                .ReturnsAsync(10);

            var result = await _controller.Create(model);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Create project sucessfully!", content.Content);
            Assert.Contains("window.location.href='/Workspace/Details/10'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_ServiceThrowsException_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                ProjectName = "New Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 2, 1 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.CreateProjectWithTeamAsync(model, 1))
                .ThrowsAsync(new Exception("Database error"));

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            var result = await _controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Error when create project: Database error",
                _controller.ModelState[""].Errors[0].ErrorMessage);
        }

        // ============================================================
        // EDIT - GET
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NotLoggedIn_RedirectsToAccessDeny()
        {
            // Current controller Edit(GET) checks RoleId first (no userId check)
            _httpContext.Session.Clear();

            var result = await _controller.Edit(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("AccessDeny", redirect.ActionName);
            Assert.Equal("Error", redirect.ControllerName);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NoUserEmail_ReturnsContent_LoginInfoNotFound()
        {
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.Remove("UserEmail");

            var result = await _controller.Edit(1);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Login information not found.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_UserNotMentor_ReturnsContent_OnlyMentorCanUpdate()
        {
            _httpContext.Session.SetInt32("UserId", 2);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "intern@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("intern@example.com"))
                .Returns(new User { UserId = 2, RoleId = 2 });

            var result = await _controller.Edit(1);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Only Mentor can update project.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ProjectNotFound_ReturnsContent_ProjectNotFound()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            _projectServicesMock
                .Setup(x => x.GetProjectForUpdateAsync(1, 1))
                .ReturnsAsync((ProjectUpdateViewModel?)null);

            var result = await _controller.Edit(1);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Project not found", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ValidProject_ReturnsViewWithModel()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var projectModel = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                ProjectName = "Test Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 2, 3, 1 },
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetProjectForUpdateAsync(1, 1))
                .ReturnsAsync(projectModel);

            var result = await _controller.Edit(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectUpdateViewModel>(viewResult.Model);

            Assert.Equal(1, model.ProjectId);
            Assert.Equal("Test Project", model.ProjectName);
        }

        // ============================================================
        // EDIT - POST
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_DeadlineInPast_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                Deadline = DateTime.Now.AddDays(-5),
                SelectedUserIds = new List<int> { 1, 2 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            _projectServicesMock
                .Setup(x => x.GetProjectMembersAsync(1))
                .ReturnsAsync(new List<ProjectMemberItem>());

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_MentorRemoved_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 }, // missing mentor id 1
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            _projectServicesMock
                .Setup(x => x.GetProjectMembersAsync(1))
                .ReturnsAsync(new List<ProjectMemberItem>());

            var result = await _controller.Edit(1, model);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("SelectedUserIds"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ValidModel_UpdateSuccess_ReturnsContent_SuccessRedirectWorkspace()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                ProjectName = "Updated Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 1, 2, 3 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
                .ReturnsAsync(true);

            var result = await _controller.Edit(1, model);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Update project sucessfully!", content.Content);
            Assert.Contains("window.location.href='/Workspace/Details/1'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_UpdateFails_ReturnsContent_CantUpdateProject()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                ProjectName = "Updated Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 1, 2 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
                .ReturnsAsync(false);

            var result = await _controller.Edit(1, model);

            var content = Assert.IsType<ContentResult>(result);
            Assert.Contains("Cant update project.", content.Content);
            Assert.Contains("window.location.href='/Project/Index'", content.Content);
        }

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
        {
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetInt32("RoleId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                ProjectName = "Updated Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 1, 2 },
                LeaderId = 2
            };

            _userServicesMock
                .Setup(x => x.GetUserById(2))
                .Returns(new User { UserId = 2, RoleId = 2 });

            _projectServicesMock
                .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
                .ThrowsAsync(new Exception("Update failed"));

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            _projectServicesMock
                .Setup(x => x.GetProjectMembersAsync(1))
                .ReturnsAsync(new List<ProjectMemberItem>());

            var result = await _controller.Edit(1, model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains("Error when update project: Update failed",
                _controller.ModelState[""].Errors[0].ErrorMessage);
        }
    }
}
