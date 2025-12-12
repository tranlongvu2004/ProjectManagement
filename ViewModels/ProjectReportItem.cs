namespace PorjectManagement.ViewModels
{
    public class ProjectReportItem
    {
        public int ReportId { get; set; }

        public int ProjectId { get; set; }

        public int LeaderId { get; set; }

        public string? ReportType { get; set; }

        public string FilePath { get; set; } = null!;
    }
}
