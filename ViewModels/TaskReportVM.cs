using System.ComponentModel.DataAnnotations;

public class TaskReportVM
{
    [Required]
    public string? Plan { get; set; }
    [Required]
    public string? Actual { get; set; }
    [Required]
    [Range(0, 100)]
    public int? ProgressPercent { get; set; }
    [Required]
    public string? Output { get; set; }
    [Required]
    public string? Issue { get; set; }
    [Required]
    public string? Action { get; set; }
    [Required]
    public string? NextPlan { get; set; }
}
