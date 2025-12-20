using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Controllers;
using PorjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

public class TimelineControllerTest
{
    private LabProjectManagementContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<LabProjectManagementContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new LabProjectManagementContext(options);
    }

    [Fact]
    public async System.Threading.Tasks.Task Timeline_WithValidProjectId_ReturnsViewAndTasks()
    {
        // Arrange
        var context = GetDbContext();

        var user = new User
        {
            UserId = 1,
            FullName = "Test User",
            Email = "test@gmail.com",
            PasswordHash = "123"
        };

        var task = new PorjectManagement.Models.Task
        {
            TaskId = 1,
            Title = "Task 1",
            ProjectId = 100,
            CreatedAt = DateTime.Now.AddDays(-2),
            Deadline = DateTime.Now.AddDays(5),
            Status = PorjectManagement.Models.TaskStatus.Doing,
            TaskAssignments = new List<TaskAssignment>
            {
                new TaskAssignment
                {
                    User = user,
                    UserId = user.UserId
                }
            }
        };

        context.Users.Add(user);
        context.Tasks.Add(task);
        context.SaveChanges();

        var controller = new TimelineController(context);

        // Act
        var result = await controller.Timeline(100); 

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.NotNull(controller.ViewBag.Tasks);

        var json = controller.ViewBag.Tasks as string;
        Assert.False(string.IsNullOrEmpty(json));

        var tasks = JsonSerializer.Deserialize<List<object>>(json!);
        Assert.Single(tasks);
    }

    [Fact]
    public async System.Threading.Tasks.Task Timeline_WithNoTasks_ReturnsEmptyList()
    {
        // Arrange
        var context = GetDbContext();
        var controller = new TimelineController(context);

        // Act
        var result = await controller.Timeline(999); 

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);

        var json = controller.ViewBag.Tasks as string;
        Assert.NotNull(json);

        var tasks = JsonSerializer.Deserialize<List<object>>(json!);
        Assert.Empty(tasks);
    }
}
