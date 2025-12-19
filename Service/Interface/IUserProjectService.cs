// Service/Interface/IUserProjectService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using PorjectManagement.Models;

namespace PorjectManagement.Service.Interface
{
    public interface IUserProjectService
    {
        bool IsleaderOfProject(int userId, int projectId);
        Task<List<User>> GetAllUsersAsync();
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<Dictionary<int, string>> AddUsersToProjectAsync(int projectId, List<int> userIds);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<User>> GetUsersByProjectIdAsync(int projectId);

        Task<List<User>> GetUsersByProjectIdNoMentorAsync(int projectId);

    }
}
