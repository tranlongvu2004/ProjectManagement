using System;

namespace PorjectManagement.Models;

public partial class TaskAttachment
{
    public int AttachmentId { get; set; }
    public int TaskId { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }

    public virtual Task Task { get; set; } = null!;
    public virtual User UploadedByNavigation { get; set; } = null!;
}