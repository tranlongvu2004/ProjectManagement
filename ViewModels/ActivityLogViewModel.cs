using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ActivityLogViewModel
    {
        public int ActivityLogId { get; set; }
        public int ProjectId { get; set; }
        public int? TaskId { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public Project Project { get; set; }
    }

}
