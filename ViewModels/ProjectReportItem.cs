using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ProjectReportItem
    {
        public int ReportId { get; set; }
        public int ProjectId { get; set; }

        public string Leader { get; set; }

        public string? ReportType { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string FilePath { get; set; } = null!;
    }
}
