// Repository/Interface/IUserProjectRepo.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IUserProjectRepo
    {
        Task<List<User>> GetAllUsersAsync();
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        System.Threading.Tasks.Task AddUsersToProjectAsync(List<UserProject> userProjects);
        Task<List<Project>> GetAllProjectsAsync();

    }
}
