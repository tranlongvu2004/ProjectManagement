using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateReportViewModel
{
    public int ProjectId { get; set; }

    [Required]
    public string ReportType { get; set; } = "daily";

    [Required]
    [MinLength(1, ErrorMessage = "At least one member is required")]
    public List<TeamMemberVM> Members { get; set; } = new();

    [Required(ErrorMessage = "Team next plan is required")]

    public DateTime? CreatedAt { get; set; }
}
