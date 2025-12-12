using PorjectManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class ProjectUpdateViewModel
    {   
        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Required]
        public ProjectStatus Status { get; set; }

        // Danh sách thành viên hiện tại
        public List<int> CurrentMemberIds { get; set; } = new();

        // Danh sách thành viên được chọn
        [Required]
        public List<int>? SelectedUserIds { get; set; }

        // Leader hiện tại và mới
        public int? CurrentLeaderId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Leader cho dự án")]
        public int? LeaderId { get; set; }

        // Danh sách user có thể chọn
        public List<AvailableUserItem> AvailableUsers { get; set; } = new();

        // Danh sách thành viên hiện tại (để hiển thị)
        public List<ProjectMemberItem> CurrentMembers { get; set; } = new();
    }
}
