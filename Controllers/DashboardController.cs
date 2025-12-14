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
        public IActionResult Dashboard(int projectId, int userId)
        { 
            
            var deletedTasks = _context.RecycleBins
                .Where(rb => rb.EntityType == "Task")
                .Select(rb => rb.EntityId).ToHashSet();
            var tasks = _context.Tasks
                .Where(t => t.ProjectId == projectId
                && t.TaskAssignments.Any(ta => ta.UserId == userId)
                && !_context.RecycleBins.Any(rb => rb.EntityType == "Task" && rb.EntityId == t.TaskId)
                && !deletedTasks.Contains(t.TaskId))
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString() ?? "Not_Started",
                    Owner = t.CreatedByNavigation.FullName ?? "Unknown"
                })
                
                .ToList();

           

            var ownerTasks = _context.Tasks
                .Where(t => t.ProjectId == projectId 
                && t.TaskAssignments.Any(ta => ta.UserId == userId)
                && !_context.RecycleBins.Any(rb => rb.EntityType == "Task" && rb.EntityId == t.TaskId)
                && !deletedTasks.Contains(t.TaskId))
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    Status = t.Status.ToString() ?? "Not_Started"
                })
                .ToList();

            int totalTasks = tasks.Count;
            int completedTasks = tasks.Count(t => t.Status == "Completed");
            int stuckTasks = tasks.Count(t => t.Status == "Stuck");
            int inProgressTasks = tasks.Count(t => t.Status == "Doing");

            ViewBag.OwnerTasks = System.Text.Json.JsonSerializer.Serialize(ownerTasks);
            ViewBag.Tasks = System.Text.Json.JsonSerializer.Serialize(tasks);   
            ViewBag.TotalTasks = totalTasks;
            ViewBag.CompletedTasks = completedTasks;
            ViewBag.StuckTasks = stuckTasks;
            ViewBag.InProgressTasks = inProgressTasks;
            ViewBag.projectId = projectId;
            ViewBag.UserId = HttpContext.Session.GetInt32("UserId") ?? 0;


            return View();
        }
        [HttpGet]
        public IActionResult GetTasks(int projectId)
        {
            var tasks = _context.Tasks
                .Select(t => new
                {
                    t.ProjectId,
                    t.Title,
                    Status = t.Status.ToString(),
                    Owner = t.CreatedByNavigation.FullName ?? "Unknown"
                })
                .Where(t => t.ProjectId == projectId)
                .ToList();

            return Json(tasks);
        }

        [HttpGet]
        public IActionResult GetTasksByUserId(int projectId, int userId)
        {
            var tasks = _context.TaskAssignments
                .Where(ta => ta.Task.ProjectId == projectId && ta.UserId == userId)
                .Include(ta => ta.Task)
                .Select(a => new {
                    a.Task.Title,
                    Status = a.Task.Status.ToString()
                })
                .ToList();

            return Json(tasks);
        }

        public IActionResult DashboardPartial(int projectId, int userId)
        {
            return View("Dashboard");
        }
        public IActionResult ExportDashboard(int projectId, int userId)
        {
            // Lấy dữ liệu Dashboard như bạn đang làm
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
