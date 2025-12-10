using System.ComponentModel.DataAnnotations.Schema;

namespace DatasetSharingPlatform.Api.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [ForeignKey("Dataset")]
        public int DatasetId { get; set; }
        public Dataset Dataset { get; set; } = null!;

        public string ReviewerName { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty; // 通过、拒绝

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Comment { get; set; } = string.Empty;
    }
}
