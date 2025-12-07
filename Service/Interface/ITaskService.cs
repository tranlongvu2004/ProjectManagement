using PorjectManagement.Models;

namespace PorjectManagement.Service.Interface
{
    public interface ITaskService
    {
        Task<int> CreateTaskAsync(Models.Task task);
        System.Threading.Tasks.Task AssignUsersToTaskAsync(int taskId, List<int> userIds);
    }
}
