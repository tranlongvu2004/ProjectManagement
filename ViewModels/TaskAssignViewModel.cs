namespace PorjectManagement.ViewModels
{
    public class TaskAssignViewModel
    {
        public int TaskId { get; set; }

        public int SelectedUserId { get; set; }
        public int ProjectId { get; set; }

        public List<UserListItemVM> Users { get; set; }
    }
}
