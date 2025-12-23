using System.ComponentModel.DataAnnotations;

public class TeamMemberVM
{
    public int UserId { get; set; }

    [Required]
    public string FullName { get; set; } = null!;

    public string? Task { get; set; }
    public string? Actual { get; set; }

    [Range(0, 100)]
    public int? ProgressPercent { get; set; }

}
