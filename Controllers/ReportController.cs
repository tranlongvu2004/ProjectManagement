using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Service.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public IActionResult ViewReport(int projectId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? roleId = HttpContext.Session.GetInt32("RoleId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            bool isLeader = _reportService.IsLeaderOfProject(userId.Value, projectId);

            if (roleId == 2 && !isLeader)
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path + HttpContext.Request.QueryString });
            var reports = _reportService.GetReportsByProjectId(projectId);
            ViewBag.ProjectId = projectId;
            return View(reports);
        }

        [HttpGet]
        public IActionResult Daily(int projectId)
        {
            int? leaderId = HttpContext.Session.GetInt32("UserId");
            if (leaderId == null) return Unauthorized();

            if (!_reportService.IsLeaderOfProject(leaderId.Value, projectId))
                return RedirectToAction("AccessDeny", "Error", new { returnUrl = HttpContext.Request.Path });

            var vm = _reportService.BuildDailyReportForm(projectId);
            vm.ReportType = "daily";

            return View(vm);
        }
        [HttpPost]
        public async Task<IActionResult> UploadDaily(CreateReportViewModel model)
        {
            int roleId = HttpContext.Session.GetInt32("RoleId") ?? 0;
            if (roleId != 2)
                return Json(new { success = false, message = "Access denied" });

            int? leaderId = HttpContext.Session.GetInt32("UserId");
            if (leaderId == null)
                return Json(new { success = false, message = "Not logged in" });

            if (!_reportService.IsLeaderOfProject(leaderId.Value, model.ProjectId))
                return Json(new { success = false, message = "Not leader" });

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid data" });

            var report = await _reportService.CreateDailyReportAsync(
                model, leaderId.Value);

            return Json(new
            {
                success = true,
                reportId = report.ReportId,
                message = "Daily report created successfully"
            });
        }

    }
}
