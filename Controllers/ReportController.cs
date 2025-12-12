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

        [HttpPost]
        public async Task<IActionResult> Upload(int projectId, string reportType, IFormFile reportFile)
        {
            if (reportFile == null)
            {
                TempData["error"] = "Vui lòng chọn file!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }

            int? leaderId = HttpContext.Session.GetInt32("UserId");
            if (leaderId == null)
            {
                TempData["error"] = "Chưa đăng nhập!";
                return RedirectToAction("Details", "Workspace", new { id = projectId });
            }

            var ok = await _reportService.UploadReportAsync(
                projectId,
                reportType,
                reportFile,
                leaderId.Value
            );

            TempData[ok ? "success" : "error"] = ok ? "Upload thành công!" : "Upload thất bại!";
            return RedirectToAction("Details", "Workspace", new { id = projectId });
        }
    }
}
