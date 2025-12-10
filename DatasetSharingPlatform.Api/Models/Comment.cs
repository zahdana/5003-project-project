namespace DatasetSharingPlatform.Api.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
