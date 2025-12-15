public class DailyReportJson
{
    public int ProjectId { get; set; }
    public int? TeamExecutePercent { get; set; }
    public string? TeamNextPlan { get; set; }

    public List<DailyMemberJson> Members { get; set; } = new();
}

public class DailyMemberJson
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;

    public string? Plan { get; set; }
    public string? Actual { get; set; }
    public int? ProgressPercent { get; set; }

    public string? Output { get; set; }
    public string? Issue { get; set; }
    public string? Action { get; set; }
    public string? NextPlan { get; set; }
}
