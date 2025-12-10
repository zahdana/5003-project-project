namespace DatasetSharingPlatform.Api.Models
{
    public class DatasetViewRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; } // 关联到 User 表
        public int DatasetId { get; set; } // 关联到 Dataset 表
        public DateTime ViewTime { get; set; } // 浏览时间
        public int ViewCount { get; set; }
        public User User { get; set; }
        public Dataset Dataset { get; set; }
    }
}
