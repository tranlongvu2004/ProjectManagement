using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace PorjectManagement.Tests.Controllers
{
    public class RecycleBinControllerTest
    {
        private readonly LabProjectManagementContext _context;
        private readonly RecycleBinController _controller;

        public RecycleBinControllerTest()
        {
            var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new LabProjectManagementContext(options);
            _controller = new RecycleBinController(_context);

            var httpContext = new DefaultHttpContext();
            httpContext.Session = new TestSession();
            httpContext.Session.SetInt32("UserId", 1);
            httpContext.Session.SetInt32("RoleId", 2);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            SeedData();
        }

        private void SeedData()
        {
            var user = new PorjectManagement.Models.User
            {
                UserId = 1,
                FullName = "Mentor A",
                Email = "mentor@gmail.com",
                PasswordHash = "123"
            };

            var task = new PorjectManagement.Models.Task
            {
                TaskId = 1,
                Title = "Deleted Task",
                ProjectId = 1,
                Status = Models.TaskStatus.Completed,
                TaskAssignments = new List<TaskAssignment>(),
                Comments = new List<Comment>(),
                TaskAttachments = new List<TaskAttachment>()
            };

            var snapshot = new DTOTaskSnapshot
            {
                TaskName = "Deleted Task",
                Owner = "Mentor A",
                Status = "Completed",
                ProjectId = 1
            };

            var recycle = new RecycleBin
            {
                RecycleId = 1,
                EntityType = "Task",
                EntityId = 1,
                DeletedBy = 1,
                DeletedAt = DateTime.Now,
                DataSnapshot = JsonSerializer.Serialize(snapshot)
            };

            _context.Users.Add(user);
            _context.Tasks.Add(task);
            _context.RecycleBins.Add(recycle);

            _context.SaveChanges();
        }

        // =====================================================
        // RecycleBin
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task RecycleBin_ReturnsViewResult_WithRecyclebinVMList()
        {
            // Act
            var result = await _controller.RecycleBin(); // ✅ await

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<RecyclebinVM>>(viewResult.Model);

            Assert.Single(model);

            var item = model.First();
            Assert.Equal("Task", item.EntityType);
            Assert.Equal("Deleted Task", item.Name);
            Assert.Equal("Mentor A", item.DeletedBy);
        }

        // =====================================================
        // Restore
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task Restore_RemovesRecycleItem_ReturnsOk()
        {
            // Arrange
            var request = new RestoreRequest { RecycleId = 1 };

            // Act
            var result = await _controller.Restore(request); // ✅ await

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Empty(_context.RecycleBins.ToList());
        }

        // =====================================================
        // DeletePermanent
        // =====================================================

        [Fact]
        public async System.Threading.Tasks.Task DeletePermanent_RemovesTaskAndRecycleBin_ReturnsOk()
        {
            // Arrange
            var request = new RestoreRequest { RecycleId = 1 };

            // Act
            var result = await _controller.DeletePermanent(request); // ✅ await

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Empty(_context.Tasks.ToList());
            Assert.Empty(_context.RecycleBins.ToList());
        }

        [Fact]
        public async System.Threading.Tasks.Task DeletePermanent_ItemNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new RestoreRequest { RecycleId = 999 };

            // Act
            var result = await _controller.DeletePermanent(request); // ✅ await

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
