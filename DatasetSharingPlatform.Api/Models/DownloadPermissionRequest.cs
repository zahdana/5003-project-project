namespace DatasetSharingPlatform.Api.Models
{
    public class DownloadPermissionRequest
    {
        public int Id { get; set; }

        public int RequesterId { get; set; }
        public User Requester { get; set; }

        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
