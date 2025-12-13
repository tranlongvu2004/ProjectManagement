using PorjectManagement.Models;

public interface IReportService
{
    bool IsLeaderOfProject(int userId, int projectId);
    public IQueryable<Report> GetReportsByProjectId(int projectId);
    Task<bool> UploadReportAsync(int projectId, string reportType, IFormFile file, int leaderId);

}
