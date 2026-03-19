namespace NurseNow.DTOs
{
    public class BrowseNursesQueryDto
    {
        public string? Search { get; set; }

        public int? ServiceCatalogId { get; set; }

        public string? Location { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}