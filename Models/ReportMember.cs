namespace PorjectManagement.Models
{
    public class ReportMember
    {
        public int ReportMemberId { get; set; }
        public int ReportId { get; set; }

        public int UserId { get; set; }
        public string FullName { get; set; } = null!;

        public string? Task { get; set; }
        public string? Actual { get; set; }
        public int? ProgressPercent { get; set; }

        public virtual Report Report { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}
