public interface IReportService
{
    Task<bool> UploadReportAsync(int projectId, string reportType, IFormFile file, int leaderId);
}
