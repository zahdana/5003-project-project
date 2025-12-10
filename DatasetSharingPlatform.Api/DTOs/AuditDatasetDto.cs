// AuditDatasetDto.cs
namespace DatasetSharingPlatform.Api.DTOs
{
    public class AuditDatasetDto
    {
        public string Action { get; set; } = string.Empty; // "approve" 或 "reject"
        public string Comment { get; set; } = string.Empty;
    }
}
