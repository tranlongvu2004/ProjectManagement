namespace PorjectManagement.ViewModels
{
    public class InternListVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
    }

}
