using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatasetSharingPlatform.Api.Models
{
    public class Dataset
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string DownloadPermission { get; set; } = "公开";

        public string FilePath { get; set; } = string.Empty;

        public string Status { get; set; } = "待审核"; // 待审核、已通过、已拒绝

        public DateTime UploadTime { get; set; } = DateTime.Now;

        // 外键关系
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<DatasetPermission> Permissions { get; set; }
        public ICollection<DatasetDownloadRecord> DatasetDownloadRecords { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<DatasetTag> DatasetTags { get; set; }
    }
}
