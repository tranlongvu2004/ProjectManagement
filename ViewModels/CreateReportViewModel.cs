using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateReportViewModel
{
    public int ProjectId { get; set; }

    [Required]
    public string ReportType { get; set; } = null!;
    [Required]
    [JsonPropertyName("Members")]
    public List<TeamMemberVM> TeamMembers { get; set; } = new();
    [Required]
    public List<TaskReportVM> Tasks { get; set; } = new();
    [Required(ErrorMessage = "Team next plan is required")]

    public string? TeamNextPlan { get; set; }
    [Required]
    [Range(0, 100)]
    public int? TeamExecutePercent { get; set; }
    public DateTime? CreatedAt { get; set; }
}
