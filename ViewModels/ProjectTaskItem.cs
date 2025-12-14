namespace PorjectManagement.ViewModels
{
    public class ProjectTaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Priority { get; set; }
        public string? Status { get; set; }
        public int? ProgressPercent { get; set; }
        public DateTime? Deadline { get; set; }
        public string? Assignees { get; set; } // Danh sách assignees, ngăn cách bằng dấu phẩy
        
        public bool? IsParent { get; set; }
    }
}