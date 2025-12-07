using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ProjectDetailDto
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = null!;
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}