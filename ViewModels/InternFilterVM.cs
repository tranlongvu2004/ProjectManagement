namespace PorjectManagement.ViewModels
{
    public class InternFilterVM
    {
        public string? Keyword { get; set; }

        public string SortBy { get; set; } = "name";      
        public string SortDir { get; set; } = "asc";      

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 7;
        public int TotalPages { get; set; }

        public List<InternListVM> Interns { get; set; } = new();
    }

}
