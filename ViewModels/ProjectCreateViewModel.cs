using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class ProjectCreateViewModel
    {
        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; } = null!;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Deadline là bắt buộc")]
        [DataType(DataType.Date)]
        public DateTime Deadline { get; set; }

        [Required]
        public List<int>? SelectedUserIds { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn Leader cho dự án")]
        public int? LeaderId { get; set; }

        public List<AvailableUserItem> AvailableUsers { get; set; } = new();
    }
}