using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class ProjectCreateViewModel
    {
        [Required(ErrorMessage = "Tên dự án là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên dự án tối đa 200 ký tự")]
        public string ProjectName { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        // Danh sách UserIds được chọn để assign vào project
        public List<int> SelectedUserIds { get; set; } = new List<int>();

        // Chọn 1 Leader từ danh sách members
        public int? LeaderId { get; set; }

        // Danh sách tất cả users (để hiển thị trong form)
        public List<AvailableUserItem>? AvailableUsers { get; set; }
    }

    // DTO cho dropdown/checkbox user selection
    public class AvailableUserItem
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string RoleName { get; set; } = null!;
    }
}