public class TeamMemberVM
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
