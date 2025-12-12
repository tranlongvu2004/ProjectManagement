using PorjectManagement.Models;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface ITaskService
    {
        Task<int> CreateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task AssignUsersToTaskAsync(int taskId, List<int> userIds);
        
        Task<bool> AssignTaskAsync(int taskId, int userId);
        Task<TaskAssignViewModel> GetAssignTaskDataAsync(int taskId);
    }
}
