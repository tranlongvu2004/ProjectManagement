using PorjectManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class TaskEditViewModel
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Tên task là bắt buộc")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public TaskPriority Priority { get; set; }

        [Required]
        public Models.TaskStatus Status { get; set; } 

        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        public List<int> CurrentAssigneeIds { get; set; } = new();

        public List<int> SelectedUserIds { get; set; } = new();

        public List<User> ProjectMembers { get; set; } = new();

        public List<TaskAssigneeItem> CurrentAssignees { get; set; } = new();
    }

    public class TaskAssigneeItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? RoleName { get; set; }
    }
}