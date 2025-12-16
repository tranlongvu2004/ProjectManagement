public class CreateReportViewModel
{
    public int ProjectId { get; set; }
    public string ReportType { get; set; } = null!;

    public List<TeamMemberVM> TeamMembers { get; set; } = new();
    public List<TaskReportVM> Tasks { get; set; } = new();

    public string? TeamNextPlan { get; set; }
    public int? TeamExecutePercent { get; set; }
}
