namespace PorjectManagement.ViewModels
{
    public class RecyclebinVM
    {
        public int RecycleId { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string DeletedBy { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }

        public int ProjectId { get; set; }
    }
}
