namespace DatasetSharingPlatform.Api.DTOs
{
    public class DatasetFilterDto
    {
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string? NameKeyword { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
