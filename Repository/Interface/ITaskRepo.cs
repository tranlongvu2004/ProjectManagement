using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface ITaskRepo
    {
        System.Threading.Tasks.Task AddTaskAsync(Models.Task task);
        Task<Models.Task?> GetTaskByIdAsync(int taskId);
    }
}
