using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;
using System.Text.Json;

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
        public bool IsLeaderOfProject(int userId, int projectId)
        {
            return _context.UserProjects.Any(up =>
                up.UserId == userId &&
                up.ProjectId == projectId &&
                up.IsLeader == true
            );
        }

        public List<CreateReportViewModel> GetReportsByProjectId(int projectId)
        {
            return _context.Reports
                .Where(r => r.ProjectId == projectId && r.ReportType == "daily" || r.ReportType == "weekly")
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => JsonSerializer.Deserialize<CreateReportViewModel>(
                    r.FilePath!,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })!
                )
                .ToList();
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
        public CreateReportViewModel BuildDailyReportForm(int projectId)
        {
            var members = _context.UserProjects
                .Where(x => x.ProjectId == projectId)
                .Select(x => new TeamMemberVM
                {
                    UserId = x.UserId,
                    FullName = x.User.FullName
                })
                .ToList();

            // ⚠️ giữ Tasks = số member để mapping index
            var tasks = members.Select(_ => new TaskReportVM()).ToList();

            return new CreateReportViewModel
            {
                ProjectId = projectId,
                TeamMembers = members,
                Tasks = tasks
            };
        }

        public async Task<Report> CreateDailyReportAsync(
     CreateReportViewModel model,
     int leaderId)
        {
            var reportJson = new DailyReportJson
            {
                ProjectId = model.ProjectId,
                TeamExecutePercent = model.TeamExecutePercent,
                TeamNextPlan = model.TeamNextPlan,
                Members = model.TeamMembers.Select((m, i) => new DailyMemberJson
                {
                    UserId = m.UserId,
                    FullName = m.FullName,

                    Plan = model.Tasks[i].Plan,
                    Actual = model.Tasks[i].Actual,
                    ProgressPercent = model.Tasks[i].ProgressPercent,
                    Output = model.Tasks[i].Output,
                    Issue = model.Tasks[i].Issue,
                    Action = model.Tasks[i].Action,
                    NextPlan = model.Tasks[i].NextPlan
                }).ToList()
            };

            var json = JsonSerializer.Serialize(reportJson);

            var report = new Report
            {
                ProjectId = model.ProjectId,
                LeaderId = leaderId,
                ReportType = "daily",
                FilePath = json,
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }
    }
}



