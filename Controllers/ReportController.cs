using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Service.Interface;

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
            if (userId == null)
                return Unauthorized();

            bool isLeader = _reportService.IsLeaderOfProject(userId.Value, projectId);

            if (!isLeader)
                return Forbid();

            var reports = _reportService
                .GetReportsByProjectId(projectId)
                .ToList();

            ViewBag.ProjectId = projectId;
            ViewBag.IsLeader = isLeader;
            return View(reports);
        }


        [HttpPost]
        public async Task<IActionResult> Upload(int projectId, string reportType, IFormFile reportFile)
        {
            int? leaderId = HttpContext.Session.GetInt32("UserId");
            if (leaderId == null)
                return Unauthorized();

            if (!_reportService.IsLeaderOfProject(leaderId.Value, projectId))
                return Forbid();


            if (reportFile == null)
            {
                TempData["error"] = "Please select a file!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }
            if (leaderId == null)
            {
                TempData["error"] = "Not logged in!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }
            var allowedExtensions = new[] { ".docx", ".pdf", ".xlsx" };
            var fileExtension = Path.GetExtension(reportFile.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
            {
                TempData["error"] = "Only .docx, .pdf, .xlsx files are allowed!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }
            const long maxFileSize = 20 * 1024 * 1024; // 20MB

            if (reportFile.Length > maxFileSize)
            {
                TempData["error"] = "File size must not exceed 20MB!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }

            var ok = await _reportService.UploadReportAsync(
                projectId,
                reportType,
                reportFile,
                leaderId.Value
            );

            TempData[ok ? "success" : "error"] = ok ? "Upload successful!" : "Upload failed!";
            return RedirectToAction("Details", "Workspace", new { id = projectId });
        }
    }
}
