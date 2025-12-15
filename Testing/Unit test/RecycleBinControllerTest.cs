using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

            SeedData();
        }

        private void SeedData()
        {
            var user = new User
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
        public void RecycleBin_ReturnsViewResult_WithRecyclebinVMList()
        {
            // Act
            var result = _controller.RecycleBin();

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
        public void Restore_RemovesRecycleItem_ReturnsOk()
        {
            // Arrange
            var request = new RestoreRequest
            {
                RecycleId = 1
            };

            // Act
            var result = _controller.Restore(request);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Empty(_context.RecycleBins.ToList());
        }

        // =====================================================
        // DeletePermanent
        // =====================================================

        [Fact]
        public void DeletePermanent_RemovesTaskAndRecycleBin_ReturnsOk()
        {
            // Arrange
            var request = new RestoreRequest
            {
                RecycleId = 1
            };

            // Act
            var result = _controller.DeletePermanent(request);

            // Assert
            Assert.IsType<OkResult>(result);
            Assert.Empty(_context.Tasks.ToList());
            Assert.Empty(_context.RecycleBins.ToList());
        }

        [Fact]
        public void DeletePermanent_ItemNotFound_ReturnsNotFound()
        {
            // Arrange
            var request = new RestoreRequest
            {
                RecycleId = 999
            };

            // Act
            var result = _controller.DeletePermanent(request);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
