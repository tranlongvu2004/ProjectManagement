using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class ProjectCreateViewModel
    {
        [Required(ErrorMessage = "Tên project không được để trống")]
        [StringLength(200, ErrorMessage = "Tên project không được quá 200 ký tự")]
        public string ProjectName { get; set; } = null!;

        [StringLength(2000, ErrorMessage = "Mô tả không được quá 2000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        // Danh sách user IDs được chọn
        public List<int>? SelectedUserIds { get; set; }

        // Leader ID (phải nằm trong SelectedUserIds)
        public int? LeaderId { get; set; }

        // Danh sách users có thể assign (để hiển thị trong form)
        public List<AvailableUserItem> AvailableUsers { get; set; } = new();
    }
}