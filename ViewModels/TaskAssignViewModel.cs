using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PorjectManagement.ViewModels
{
    public class TaskAssignViewModel
    {
        public int TaskId { get; set; }

        public int SelectedUserId { get; set; }
        public int ProjectId { get; set; }
        [ValidateNever]
        public List<UserListItemVM> Users { get; set; }
    }
}
