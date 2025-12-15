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
using System.Linq;
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
        // CREATE - GET TESTS
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_NotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _httpContext.Session.Clear();

            // Act
            var result = await _controller.Create();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Get_NoUserEmail_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    // No UserEmail set

        //    // Act
        //    var result = await _controller.Create();

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("No information login.", _controller.TempData["Error"]);
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Get_UserIsNull_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "test@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("test@example.com"))
        //        .Returns((User?)null);

        //    // Act
        //    var result = await _controller.Create();

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Only Mentor can create project.", _controller.TempData["Error"]);
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Get_UserNotMentor_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "intern@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("intern@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 2, Email = "intern@example.com" });

        //    // Act
        //    var result = await _controller.Create();

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Only Mentor can create project.", _controller.TempData["Error"]);
        //}

        [Fact]
        public async System.Threading.Tasks.Task Create_Get_ValidMentor_ReturnsViewWithModel()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
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

            // Act
            var result = await _controller.Create();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectCreateViewModel>(viewResult.Model);

            Assert.NotNull(model.AvailableUsers);
            Assert.Single(model.AvailableUsers);
            Assert.True(model.Deadline > DateTime.Now);
        }

        // ============================================================
        // CREATE - POST TESTS
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _httpContext.Session.Clear();
            var model = new ProjectCreateViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Post_UserNotMentor_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "intern@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("intern@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 2 });

        //    var model = new ProjectCreateViewModel();

        //    // Act
        //    var result = await _controller.Create(model);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("No permission to create a project.", _controller.TempData["Error"]);
        //}

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_DeadlineInPast_ReturnsViewWithError()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
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

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NoMembers_ReturnsViewWithError()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
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

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("SelectedUserIds"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_NoLeader_ReturnsViewWithError()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
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

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("LeaderId"));
        }

        [Fact]
        public async System.Threading.Tasks.Task Create_Post_LeaderNotInMembers_ReturnsViewWithError()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectCreateViewModel
            {
                Deadline = DateTime.Now.AddDays(10),
                SelectedUserIds = new List<int> { 2, 3 },
                LeaderId = 4 // Not in SelectedUserIds
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            // Act
            var result = await _controller.Create(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("LeaderId"));
        }

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Post_ValidModel_RedirectsToWorkspace()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectCreateViewModel
        //    {
        //        ProjectName = "New Project",
        //        Description = "Test Description",
        //        Deadline = DateTime.Now.AddDays(30),
        //        SelectedUserIds = new List<int> { 2, 3 },
        //        LeaderId = 2
        //    };

        //    _projectServicesMock
        //        .Setup(x => x.CreateProjectWithTeamAsync(model, 1))
        //        .ReturnsAsync(10);

        //    // Act
        //    var result = await _controller.Create(model);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Details", redirect.ActionName);
        //    Assert.Equal("Workspace", redirect.ControllerName);
        //    Assert.Equal(10, redirect.RouteValues["id"]);
        //    Assert.Equal("Create project sucessfully!", _controller.TempData["Success"]);

        //    _projectServicesMock.Verify(
        //        x => x.CreateProjectWithTeamAsync(model, 1),
        //        Times.Once
        //    );
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Create_Post_ServiceThrowsException_ReturnsViewWithError()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectCreateViewModel
        //    {
        //        ProjectName = "New Project",
        //        Deadline = DateTime.Now.AddDays(30),
        //        SelectedUserIds = new List<int> { 2 },
        //        LeaderId = 2
        //    };

        //    _projectServicesMock
        //        .Setup(x => x.CreateProjectWithTeamAsync(model, 1))
        //        .ThrowsAsync(new Exception("Database error"));

        //    _projectServicesMock
        //        .Setup(x => x.GetAvailableUsersAsync())
        //        .ReturnsAsync(new List<AvailableUserItem>());

        //    // Act
        //    var result = await _controller.Create(model);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    Assert.False(_controller.ModelState.IsValid);
        //    Assert.Contains("error when create project: Database error", 
        //        _controller.ModelState[""].Errors[0].ErrorMessage);
        //}

        // ============================================================
        // EDIT - GET TESTS
        // ============================================================

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_NotLoggedIn_RedirectsToLogin()
        {
            // Arrange
            _httpContext.Session.Clear();

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirect.ActionName);
            Assert.Equal("User", redirect.ControllerName);
        }

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Get_UserNotMentor_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "intern@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("intern@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 2 });

        //    // Act
        //    var result = await _controller.Edit(1);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Only Mentor have permission create project.", _controller.TempData["Error"]);
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Get_ProjectNotFound_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    _projectServicesMock
        //        .Setup(x => x.GetProjectForUpdateAsync(1, 1))
        //        .ReturnsAsync((ProjectUpdateViewModel?)null);

        //    // Act
        //    var result = await _controller.Edit(1);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Project not found, or you do not have editing permissions..", _controller.TempData["Error"]);
        //}

        [Fact]
        public async System.Threading.Tasks.Task Edit_Get_ValidProject_ReturnsViewWithModel()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var projectModel = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                ProjectName = "Test Project",
                Deadline = DateTime.Now.AddDays(30),
                SelectedUserIds = new List<int> { 2, 3 },
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetProjectForUpdateAsync(1, 1))
                .ReturnsAsync(projectModel);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectUpdateViewModel>(viewResult.Model);
            Assert.Equal(1, model.ProjectId);
            Assert.Equal("Test Project", model.ProjectName);
        }

        // ============================================================
        // EDIT - POST TESTS
        // ============================================================

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Post_IdMismatch_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectUpdateViewModel { ProjectId = 5 };

        //    // Act
        //    var result = await _controller.Edit(1, model); // id = 1, but model.ProjectId = 5

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Data not valid.", _controller.TempData["Error"]);
        //}

        [Fact]
        public async System.Threading.Tasks.Task Edit_Post_DeadlineInPast_ReturnsViewWithError()
        {
            // Arrange
            _httpContext.Session.SetInt32("UserId", 1);
            _httpContext.Session.SetString("UserEmail", "mentor@example.com");

            _userServicesMock
                .Setup(x => x.GetUser("mentor@example.com"))
                .Returns(new User { UserId = 1, RoleId = 1 });

            var model = new ProjectUpdateViewModel
            {
                ProjectId = 1,
                Deadline = DateTime.Now.AddDays(-5),
                SelectedUserIds = new List<int> { 2 },
                LeaderId = 2
            };

            _projectServicesMock
                .Setup(x => x.GetAvailableUsersAsync())
                .ReturnsAsync(new List<AvailableUserItem>());

            _projectServicesMock
                .Setup(x => x.GetProjectMembersAsync(1))
                .ReturnsAsync(new List<ProjectMemberItem>());

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ContainsKey("Deadline"));
        }

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Post_ValidModel_UpdateSuccess_RedirectsToWorkspace()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectUpdateViewModel
        //    {
        //        ProjectId = 1,
        //        ProjectName = "Updated Project",
        //        Deadline = DateTime.Now.AddDays(30),
        //        SelectedUserIds = new List<int> { 2, 3 },
        //        LeaderId = 2
        //    };

        //    _projectServicesMock
        //        .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
        //        .ReturnsAsync(true);

        //    // Act
        //    var result = await _controller.Edit(1, model);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Details", redirect.ActionName);
        //    Assert.Equal("Workspace", redirect.ControllerName);
        //    Assert.Equal(1, redirect.RouteValues["id"]);
        //    Assert.Equal("Update project sucessfully!", _controller.TempData["Success"]);

        //    _projectServicesMock.Verify(
        //        x => x.UpdateProjectWithTeamAsync(model, 1),
        //        Times.Once
        //    );
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Post_UpdateFails_RedirectsToProject()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectUpdateViewModel
        //    {
        //        ProjectId = 1,
        //        ProjectName = "Updated Project",
        //        Deadline = DateTime.Now.AddDays(30),
        //        SelectedUserIds = new List<int> { 2 },
        //        LeaderId = 2
        //    };

        //    _projectServicesMock
        //        .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
        //        .ReturnsAsync(false);

        //    // Act
        //    var result = await _controller.Edit(1, model);

        //    // Assert
        //    var redirect = Assert.IsType<RedirectToActionResult>(result);
        //    Assert.Equal("Index", redirect.ActionName);
        //    Assert.Equal("Project", redirect.ControllerName);
        //    Assert.Equal("Cannt update project.", _controller.TempData["Error"]);
        //}

        //[Fact]
        //public async System.Threading.Tasks.Task Edit_Post_ServiceThrowsException_ReturnsViewWithError()
        //{
        //    // Arrange
        //    _httpContext.Session.SetInt32("UserId", 1);
        //    _httpContext.Session.SetString("UserEmail", "mentor@example.com");

        //    _userServicesMock
        //        .Setup(x => x.GetUser("mentor@example.com"))
        //        .Returns(new User { UserId = 1, RoleId = 1 });

        //    var model = new ProjectUpdateViewModel
        //    {
        //        ProjectId = 1,
        //        ProjectName = "Updated Project",
        //        Deadline = DateTime.Now.AddDays(30),
        //        SelectedUserIds = new List<int> { 2 },
        //        LeaderId = 2
        //    };

        //    _projectServicesMock
        //        .Setup(x => x.UpdateProjectWithTeamAsync(model, 1))
        //        .ThrowsAsync(new Exception("Update failed"));

        //    _projectServicesMock
        //        .Setup(x => x.GetAvailableUsersAsync())
        //        .ReturnsAsync(new List<AvailableUserItem>());

        //    _projectServicesMock
        //        .Setup(x => x.GetProjectMembersAsync(1))
        //        .ReturnsAsync(new List<ProjectMemberItem>());

        //    // Act
        //    var result = await _controller.Edit(1, model);

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);
        //    Assert.False(_controller.ModelState.IsValid);
        //    Assert.Contains("Error when update project: Update failed",
        //        _controller.ModelState[""].Errors[0].ErrorMessage);
        //}
    }
}