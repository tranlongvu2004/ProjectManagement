using PorjectManagement.Models;

namespace PorjectManagement.ViewModels
{
    public class ProjectFilterVM
    {
        // Filter
        public string? Keyword { get; set; }
        public ProjectStatus? Status { get; set; }

        // Sort
        public string? SortBy { get; set; } // name, deadline
        public string? SortDir { get; set; } // asc, desc

        // Paging
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 7;

        public int TotalPages { get; set; }
        public List<ProjectListVM> Projects { get; set; } = new();
    }
}
