using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ProjectReportItem
    {
        public int ReportId { get; set; }
        public int ProjectId { get; set; }

        public string Leader { get; set; } = null!;
        public string ReportType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int TeamExecutePercent { get; set; }
        public string TeamNextPlan { get; set; } = null!;

        public List<TeamMemberVM> Members { get; set; } = new();
    }
}
