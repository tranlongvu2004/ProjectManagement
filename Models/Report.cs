using System;
using System.Collections.Generic;

namespace PorjectManagement.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int ProjectId { get; set; }

    public int LeaderId { get; set; }

    public string? ReportType { get; set; }

    public string FilePath { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual User Leader { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;
}
