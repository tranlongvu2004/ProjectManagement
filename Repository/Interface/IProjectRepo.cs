using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IProjectRepo
    {
        Task<List<Project>> GetProjectsOfUserAsync(int userId);
        Task<Project?> GetByIdAsync(int projectId);
    }
}
