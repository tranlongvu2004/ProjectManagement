using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Fake HttpContext + Session
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
            var user = new User
            {
                UserId = 1,
                FullName = "Mentor A"
            };

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

            _context.Users.Add(user);
            _context.Tasks.AddRange(task1, task2);

            _context.TaskAssignments.Add(new TaskAssignment
            {
                TaskId = 1,
                UserId = 1,
                Task = task1
            });

            _context.SaveChanges();
        }

        // Dashboard

        [Fact]
        public void Dashboard_ReturnsViewResult_WithStatistics()
        {
            // Act
            var result = _controller.Dashboard(1, 1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);

            Assert.Equal(2, _controller.ViewBag.TotalTasks);
            Assert.Equal(1, _controller.ViewBag.CompletedTasks);
            Assert.Equal(1, _controller.ViewBag.InProgressTasks);
        }

        // GetTasks

        [Fact]
        public void GetTasks_ReturnsJson_WithTasksByProject()
        {
            // Act
            var result = _controller.GetTasks(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Equal(2, data.Count());
        }

        // GetTasksByUserId

        [Fact]
        public void GetTasksByUserId_ReturnsJson_WithUserTasks()
        {
            // Act
            var result = _controller.GetTasksByUserId(1, 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var data = Assert.IsAssignableFrom<IEnumerable<object>>(jsonResult.Value);

            Assert.Single(data);
        }
    }
}
