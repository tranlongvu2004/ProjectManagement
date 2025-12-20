using Microsoft.AspNetCore.Mvc;
using PorjectManagement.Models;

namespace PorjectManagement.Service.Interface
{
    public interface ITaskHistoryService
    {
        System.Threading.Tasks.Task AddAsync(int taskId, int userId, string action, string description);
        Task<List<TaskHistory>> GetByTaskIdAsync(int taskId);
        Task<List<TaskHistory>> GetAllAsync();

    }
}
