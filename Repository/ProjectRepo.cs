using PorjectManagement.Repository.Interface;
using PorjectManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace PorjectManagement.Repository
{
    public class ProjectRepo : IProjectRepo
    {
        private readonly LabProjectManagementContext _context;

        public ProjectRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetProjectsOfUserAsync(int userId)
        {
            return await _context.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User)
                .Where(p => p.UserProjects.Any(up => up.UserId == userId))
                .ToListAsync();
        }


        public async Task<Project?> GetByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }
    }
}
