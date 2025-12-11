namespace PorjectManagement.Models
{
    public partial class RecycleBin
    {
        public int RecycleId { get; set; }

        public string EntityType { get; set; } = null!;

        public int EntityId { get; set; }

        public string DataSnapshot { get; set; } = null!;

        public int? DeletedBy { get; set; }

        public DateTime? DeletedAt { get; set; }
    }

}
