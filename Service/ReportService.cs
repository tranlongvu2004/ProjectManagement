using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;

namespace PorjectManagement.Service
{
    public class ReportService : IReportService
    {
        private readonly LabProjectManagementContext _context;
        private readonly IWebHostEnvironment _env;

        public ReportService(LabProjectManagementContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<bool> UploadReportAsync(int projectId, string reportType, IFormFile file, int leaderId)
        {
            if (file == null || file.Length == 0)
                return false;

            // Tạo folder nếu chưa có
            var folder = Path.Combine(_env.WebRootPath, "reports");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Tên file
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(folder, fileName);

            // Lưu file vật lý
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Lưu DB
            var report = new Report
            {
                ProjectId = projectId,
                LeaderId = leaderId,
                ReportType = reportType.ToLower(),
                FilePath = "/reports/" + fileName,
                CreatedAt = DateTime.Now
            };

            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
