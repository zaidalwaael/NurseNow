using System.ComponentModel.DataAnnotations;

namespace NurseNow.DTOs
{
    public class BrowseNursesQueryDto
    {
        public string? Search { get; set; }
        public int? ServiceCatalogId { get; set; }
        public string? Location { get; set; }

        [Range(1, 100000)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
