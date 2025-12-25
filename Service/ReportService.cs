using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service
{
    public class ReportService : IReportService
    {
        private readonly LabProjectManagementContext _context;

        public ReportService(LabProjectManagementContext context)
        {
            _context = context;
        }

        // =====================================================
        // CHECK LEADER
        // =====================================================
        public bool IsLeaderOfProject(int userId, int projectId)
        {
            return _context.UserProjects.Any(up =>
                up.UserId == userId &&
                up.ProjectId == projectId &&
                up.IsLeader == true
            );
        }

        // =====================================================
        // GET REPORTS BY PROJECT
        // =====================================================
        public List<CreateReportViewModel> GetReportsByProjectId(int projectId)
        {
            var reports = _context.Reports
                .Include(r => r.Members)
                .Where(r =>
                    r.ProjectId == projectId &&
                    (r.ReportType == "daily")
                )
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return reports.Select(r => new CreateReportViewModel
            {
                ProjectId = r.ProjectId,
                ReportType = r.ReportType,
                CreatedAt = r.CreatedAt,

                Members = r.Members.Select(m => new TeamMemberVM
                {
                    UserId = m.UserId,
                    FullName = m.FullName,
                    Task = m.Task,
                    Actual = m.Actual,
                    ProgressPercent = m.ProgressPercent,
                }).ToList()
            }).ToList();
        }

        // =====================================================
        // BUILD DAILY REPORT FORM
        // =====================================================
        public CreateReportViewModel BuildDailyReportForm(int projectId)
        {
            var members = _context.UserProjects
                .Where(x => x.ProjectId == projectId && x.User.Role.RoleId == 2)
                .Select(x => new TeamMemberVM
                {
                    UserId = x.UserId,
                    FullName = x.User.FullName
                })
                .ToList();

            return new CreateReportViewModel
            {
                ProjectId = projectId,
                ReportType = "daily",
                Members = members
            };
        }

        // =====================================================
        // CREATE DAILY REPORT
        // =====================================================
        public async Task<Report> CreateDailyReportAsync(
            CreateReportViewModel model,
            int leaderId)
        {
            var report = new Report
            {
                ProjectId = model.ProjectId,
                LeaderId = leaderId,
                ReportType = "daily",
                CreatedAt = DateTime.Now
            };

            foreach (var m in model.Members)
            {
                report.Members.Add(new ReportMember
                {
                    UserId = m.UserId,
                    FullName = m.FullName,
                    Task = m.Task,
                    Actual = m.Actual,
                    ProgressPercent = m.ProgressPercent
                });
            }

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }
    }
}
