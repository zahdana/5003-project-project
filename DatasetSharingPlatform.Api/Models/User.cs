namespace DatasetSharingPlatform.Api.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<DatasetPermission> DatasetPermissions { get; set; }
        public ICollection<DatasetDownloadRecord> DatasetDownloadRecords { get; set; }
    }
}
