using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class TaskCreateViewModel
    {
        public int ProjectId { get; set; }

        public string Title { get; set; }
        public string? Description { get; set; }

        public TaskPriority Priority { get; set; } 

        public DateTime? Deadline { get; set; }

        public List<int> SelectedUserIds { get; set; } = new();

        public List<User> ProjectMembers { get; set; } = new();
        public bool IsSubTask { get; set; }   // checkbox

        public int? ParentTaskId { get; set; } // dropdown chọn cha

        [ValidateNever]
        public List<SelectListItem> ParentTasks { get; set; } = new();

    }
}
