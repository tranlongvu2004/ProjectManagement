namespace PorjectManagement.Models
{
    public class DTOTaskSnapshot
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public string Owner { get; set; }
        public int ProjectId { get; set; }
    }
}
