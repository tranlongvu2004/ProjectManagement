using Microsoft.EntityFrameworkCore;
using PorjectManagement.Models;
using PorjectManagement.Repository.Interface;
using PorjectManagement.ViewModels;

namespace PorjectManagement.Repository
{
    public class ProjectRepo : IProjectRepo
    {
        private readonly LabProjectManagementContext _context;

        public ProjectRepo(LabProjectManagementContext context)
        {
            _context = context;
        }

        // ===== Methods từ HEAD =====
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects
                .Include(p => p.UserProjects)
                    .ThenInclude(up => up.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProjectDetailDto?> GetProjectByIdAsync(int projectId)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.ProjectId == projectId)
                .FirstOrDefaultAsync();

            if (project == null)
                return null;

            return new ProjectDetailDto
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                Description = project.Description,
                Status = project.Status?.ToString(),
                Deadline = project.Deadline,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        public async Task<List<ProjectMemberItem>> GetProjectMembersAsync(int projectId)
        {
            var query =
                from up in _context.UserProjects
                join u in _context.Users on up.UserId equals u.UserId
                join r in _context.Roles on u.RoleId equals r.RoleId
                where up.ProjectId == projectId
                select new ProjectMemberItem
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    RoleName = r.RoleName,
                    IsLeader = up.IsLeader ?? false
                };

            return await query.ToListAsync();
        }

        // Method dev/Vu
        public Task<List<ProjectReportItem>> GetProjectReportAsync(int projectId)
        {
            var query = from r in _context.Reports
                        where r.ProjectId == projectId
                        select new ProjectReportItem
                        {
                            ReportId = r.ReportId,
                            ProjectId = r.ProjectId,
                            Leader = r.Leader.FullName,
                            ReportType = r.ReportType,
                            FilePath = r.FilePath
                        };
            return query.ToListAsync();
        }

        public async Task<List<ProjectTaskItem>> GetProjectTasksAsync(int projectId)
        {
            var tasksQuery =
                from t in _context.Tasks
                where t.ProjectId == projectId
                select new ProjectTaskItem
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    Priority = t.Priority.HasValue
                        ? t.Priority.Value.ToString()
                        : null,
                    Status = t.Status.HasValue
                        ? t.Status.Value.ToString()
                        : null,
                    ProgressPercent = t.ProgressPercent,
                    Deadline = t.Deadline,
                    IsParent = t.IsParent, // check field IsParent
                    Assignees = string.Join(", ",
                                      from ta in _context.TaskAssignments
                                      join u in _context.Users on ta.UserId equals u.UserId
                                      where ta.TaskId == t.TaskId
                                      select u.FullName)
                };

            return await tasksQuery.ToListAsync();
        }

        // ✅ Method từ dev/Vu - GIỮ NGUYÊN (có thêm Reports)
        public async Task<ProjectWorkspaceViewModel?> GetWorkspaceAsync(int projectId)
        {
            var project = await GetProjectByIdAsync(projectId);
            if (project == null)
            {
                return null;
            }

            var members = await GetProjectMembersAsync(projectId);
            var tasks = await GetProjectTasksAsync(projectId);
            var report = await GetProjectReportAsync(projectId);
            
            // Tính progress dựa trên số task Completed
            var overallProgress = 0;
            if (tasks.Any())
            {
                var parentTasks = tasks.Where(t => t.IsParent != false).ToList();
                
                if (parentTasks.Any())
                {
                    var completedCount = parentTasks.Count(t => 
                        t.Status != null && 
                        t.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)
                    );
                    
                    var totalCount = parentTasks.Count;
                    
                    // Progress = (Completed / Total) * 100
                    overallProgress = (int)Math.Round((double)completedCount / totalCount * 100);
                }
            }

            return new ProjectWorkspaceViewModel
            {
                Project = project,
                Members = members,
                Tasks = tasks,
                Reports = report,
                OverallProgress = overallProgress
            };
        }

        public async Task<int> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project.ProjectId;
        }

        // ===== Methods từ dev/Vu - GIỬ NGUYÊN =====
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

        public async Task<Project?> GetProjectEntityByIdAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.UserProjects)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId);
        }

        public async System.Threading.Tasks.Task UpdateProjectAsync(Project project)
        {
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
