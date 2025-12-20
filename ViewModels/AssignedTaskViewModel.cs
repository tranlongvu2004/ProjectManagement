namespace PorjectManagement.ViewModels
{
    public class AssignedTaskViewModel
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string TaskTitle { get; set; }
        public string ProjectName { get; set; }
        public DateTime? Deadline { get; set; }
        public TaskStatus Status { get; set; }
    }

}
