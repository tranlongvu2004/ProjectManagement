using System.Collections.Generic;
using System.Threading.Tasks;
using PorjectManagement.Models;

namespace PorjectManagement.Repository.Interface
{
    public interface IUserProjectRepo
    {
        bool IsleaderOfProject(int userId, int projectId);
        
        Task<List<User>> GetAllUsersAsync();
        Task<Project?> GetProjectByIdAsync(int projectId);
        Task<bool> IsUserInProjectAsync(int userId, int projectId);
        Task<List<Project>> GetAllProjectsAsync();
        Task<List<User>> GetUsersByProjectIdAsync(int projectId);
        
        System.Threading.Tasks.Task AddUsersToProjectAsync(List<UserProject> userProjects);
        System.Threading.Tasks.Task AddMembersToProjectAsync(int projectId, List<int> selectedUserIds, int? leaderId);
        
        System.Threading.Tasks.Task RemoveAllMembersFromProjectAsync(int projectId);
    }
}
