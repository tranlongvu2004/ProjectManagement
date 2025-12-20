using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class DashboardControllerTest
    {
        private readonly LabProjectManagementContext _context;
        private readonly DashboardController _controller;

        public DashboardControllerTest()
        {
            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new LabProjectManagementContext(options);
            _controller = new DashboardController(_context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            SeedData();
        }

        private void SeedData()
        {
            // User (phải có Email + PasswordHash do model required)
            var user = new PorjectManagement.Models.User
            {
                UserId = 1,
                FullName = "Mentor A",
                Email = "mentora@test.com",
                PasswordHash = "fake_hash"
            };

            _context.Users.Add(user);

            // UserProjects (Dashboard() lấy usersInProject từ đây)
            _context.UserProjects.Add(new UserProject
            {
                UserId = 1,
                ProjectId = 1,
                User = user
            });

            // Tasks
            var task1 = new PorjectManagement.Models.Task
            {
                TaskId = 1,
                Title = "Task 1",
                ProjectId = 1,
                Status = Models.TaskStatus.Completed,
                CreatedByNavigation = user
            };

            var task2 = new PorjectManagement.Models.Task
            {
                TaskId = 2,
                Title = "Task 2",
                ProjectId = 1,
                Status = Models.TaskStatus.Doing,
                CreatedByNavigation = user
            };

            _context.Tasks.AddRange(task1, task2);

            // TaskAssignments 
            _context.TaskAssignments.AddRange(
                new TaskAssignment
                {
                    TaskId = 1,
                    UserId = 1,
                    Task = task1,
                    User = user
                },
                new TaskAssignment
                {
                    TaskId = 2,
                    UserId = 1,
                    Task = task2,
                    User = user
                }
            );

            _context.SaveChanges();
        }

        // Dashboard

        [Fact]
        public async System.Threading.Tasks.Task Dashboard_ReturnsViewResult_WithStatistics()
        {
            // Act
            var result = await _controller.Dashboard(1, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.Equal(2, _controller.ViewBag.TotalTasks);
            Assert.Equal(1, _controller.ViewBag.CompletedTasks);
            Assert.Equal(1, _controller.ViewBag.InProgressTasks);
        }

        // GetTasks

        [Fact]
        public async System.Threading.Tasks.Task GetTasks_ReturnsJson_WithTasksByProject()
        {
            // Act
            var result = await _controller.GetTasks(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Equal(2, data.Count());
        }

        // GetTasksByUserId

        [Fact]
        public async System.Threading.Tasks.Task GetTasksByUserId_ReturnsJson_WithUserTasks()
        {
            // Act
            var result = await _controller.GetTasksByUserId(1, 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Equal(2, data.Count());
        }
    }
}
