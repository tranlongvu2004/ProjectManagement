using PorjectManagement.Models;
using PorjectManagement.ViewModels;

public interface IReportService
{
    bool IsLeaderOfProject(int userId, int projectId);
    List<CreateReportViewModel> GetReportsByProjectId(int projectId);
    Task<bool> UploadReportAsync(int projectId, string reportType, IFormFile file, int leaderId);

    CreateReportViewModel BuildDailyReportForm(int projectId);
    Task<Report> CreateDailyReportAsync(
        CreateReportViewModel model,
        int leaderId);

}
