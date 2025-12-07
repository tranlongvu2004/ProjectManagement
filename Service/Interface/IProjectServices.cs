using PorjectManagement.Models.ViewModels;

namespace PorjectManagement.Service.Interface
{
    public interface IProjectServices
    {
        Task<List<ProjectListVM>> GetProjectsOfUserAsync(int userId);
    }
}
