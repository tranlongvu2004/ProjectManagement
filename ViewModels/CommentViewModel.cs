using System.ComponentModel.DataAnnotations;

namespace PorjectManagement.ViewModels
{
    public class CommentDisplayViewModel
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        
        public string UserFullName { get; set; } = null!;
        public string? UserAvatarUrl { get; set; }
        public string RoleName { get; set; } = null!;
    }

    public class CommentCreateViewModel
    {
        public int TaskId { get; set; }
        
        [Required(ErrorMessage = "Comment cannot be empty")]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Content { get; set; } = null!;
    }
}
