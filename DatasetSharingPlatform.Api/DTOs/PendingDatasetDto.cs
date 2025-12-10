namespace DatasetSharingPlatform.Api.DTOs
{
    public class PendingDatasetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime UploadTime { get; set; }
        public string UploaderUsername { get; set; } = string.Empty;
    }
}
