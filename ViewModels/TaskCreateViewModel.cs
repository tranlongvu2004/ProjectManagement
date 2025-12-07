using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class TaskCreateViewModel
    {
        public int ProjectId { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Low;

        public DateTime? Deadline { get; set; }

        public List<int> SelectedUserIds { get; set; } = new();

        public List<User> ProjectMembers { get; set; } = new();
    }
}
