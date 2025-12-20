using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PorjectManagement.Models;
using System.ComponentModel;

namespace PorjectManagement.Controllers
{
    public class DashboardController : Controller
    {
        protected LabProjectManagementContext _context;
        public DashboardController(LabProjectManagementContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Dashboard(int projectId, int userId)
        { 
            int currentUserId = HttpContext.Session.GetInt32("UserId") ?? 0;
            if (currentUserId == 0)
            {
                return RedirectToAction("Login", "User");
            }

            var usersInProject = await _context.UserProjects
        .Include(up => up.User)
        .Where(up => up.ProjectId == projectId)
        .Select(up => up.User)
        .ToListAsync();

            var taskAssignments = await _context.TaskAssignments
       .Include(ta => ta.Task)
       .Where(ta =>
           ta.Task.ProjectId == projectId &&
           !_context.RecycleBins.Any(rb =>
               rb.EntityType == "Task" && rb.EntityId == ta.TaskId))
       .ToListAsync();


            var tasksForChart = usersInProject.SelectMany(u =>
            {
                var userTasks = taskAssignments
                    .Where(ta => ta.UserId == u.UserId)
                    .Select(ta => new
                    {
                        ProjectId = projectId,
                        Title = ta.Task.Title,
                        Status = ta.Task.Status.ToString(),
                        Owner = u.FullName
                    })
                    .ToList();

                return userTasks;
            }).ToList();


            var tasks = await _context.Tasks
                .Include(t => t.TaskAssignments)
                .ThenInclude(ta => ta.User)
                .Include(t => t.CreatedByNavigation)
                .Where(t => t.ProjectId == projectId
                    && !_context.RecycleBins.Any(rb =>
                    rb.EntityType == "Task" && rb.EntityId == t.TaskId))
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString(),
                    Owner = t.TaskAssignments
                        .Select(ta => ta.User.FullName)
                        .FirstOrDefault() ?? "Unassigned"
                })
                .ToListAsync();

           

            var ownerTasks = await _context.Tasks
                .Where(t => t.ProjectId == projectId 
                && t.TaskAssignments.Any(ta => ta.UserId == userId)
                && !_context.RecycleBins.Any(rb => rb.EntityType == "Task" && rb.EntityId == t.TaskId))
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Status = t.Status.ToString() ?? "Not_Started"
                })
                .ToListAsync();

            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int stuckTasks = tasks.Count(t => t.Status == "Stuck");
            int inProgressTasks = tasks.Count(t => t.Status == "Doing");

            ViewBag.OwnerTasks = System.Text.Json.JsonSerializer.Serialize(ownerTasks);
            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(tasks);
            ViewBag.TasksForChart = System.Text.Json.JsonSerializer.Serialize(tasksForChart);
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.StuckTasks = stuckTasks;
            ViewBag.InProgressTasks = inProgressTasks;
            ViewBag.projectId = projectId;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;


            return View();
        }
        [HttpGet]
        public async Task<IActionResult> GetTasks(int projectId)
        {
            var tasks = await _context.TaskAssignments
                .Include(ta => ta.User)
                .Include(ta => ta.Task)
                .Where(ta => ta.Task.ProjectId == projectId)
                .Select(ta => new
                {
                    ta.Task.Title,
                    Status = ta.Task.Status.ToString(),
                    Owner = ta.User.FullName
                })
                .ToListAsync();

            return Json(tasks);
        }


        [HttpGet]
        public async Task<IActionResult> GetTasksByUserId(int projectId, int userId)
        {
            var tasks = await _context.TaskAssignments
                .Where(ta => ta.Task.ProjectId == projectId && ta.UserId == userId)
                .Include(ta => ta.Task)
                .Select(a => new {
                    a.Task.Title,
                    Status = a.Task.Status.ToString()
                })
                .ToListAsync();

            return Json(tasks);
        }

        public IActionResult DashboardPartial(int projectId, int userId)
        {
            return View("Dashboard");
        }
        public IActionResult ExportDashboard(int projectId, int userId)
        {
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Status = t.Status.ToString(),
                    Owner = t.CreatedByNavigation.FullName
                })
                .ToList();

            var ownerTasks = _context.Tasks
                .Where(t => t.ProjectId == projectId &&
                            t.TaskAssignments.Any(ta => ta.UserId == userId))
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Status = t.Status.ToString()
                })
                .ToList();

            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int stuckTasks = tasks.Count(t => t.Status == "Stuck");
            int inProgressTasks = tasks.Count(t => t.Status == "Doing");

            // Tạo file Excel
            using var package = new ExcelPackage();

            var ws = package.Workbook.Worksheets.Add("Summary");
            ws.Cells["A1"].Value = "Metric";
            ws.Cells["B1"].Value = "Value";
            ws.Cells["A2"].Value = "TotalTasks";
            ws.Cells["B2"].Value = totalTasks;
            ws.Cells["A3"].Value = "Completed";
            ws.Cells["B3"].Value = completedTasks;
            ws.Cells["A4"].Value = "Stuck";
            ws.Cells["B4"].Value = stuckTasks;
            ws.Cells["A5"].Value = "Doing";
            ws.Cells["B5"].Value = inProgressTasks;
            ws.Cells.AutoFitColumns();

            // Sheet AllTasks
            var ws2 = package.Workbook.Worksheets.Add("AllTasks");
            ws2.Cells[1, 1].Value = "TaskId";
            ws2.Cells[1, 2].Value = "Title";
            ws2.Cells[1, 3].Value = "Status";
            ws2.Cells[1, 4].Value = "Owner";
            int row = 2;
            foreach (var t in tasks)
            {
                ws2.Cells[row, 1].Value = t.TaskId;
                ws2.Cells[row, 2].Value = t.Title;
                ws2.Cells[row, 3].Value = t.Status;
                ws2.Cells[row, 4].Value = t.Owner;
                row++;
            }
            ws2.Cells.AutoFitColumns();

            // Sheet OwnerTasks
            var ws3 = package.Workbook.Worksheets.Add("OwnerTasks");
            ws3.Cells[1, 1].Value = "TaskId";
            ws3.Cells[1, 2].Value = "Title";
            ws3.Cells[1, 3].Value = "Status";
            row = 2;
            foreach (var t in ownerTasks)
            {
                ws3.Cells[row, 1].Value = t.TaskId;
                ws3.Cells[row, 2].Value = t.Title;
                ws3.Cells[row, 3].Value = t.Status;
                row++;
            }
            ws3.Cells.AutoFitColumns();

            var fileBytes = package.GetAsByteArray();
            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Dashboard.xlsx");
        }


    }
}
