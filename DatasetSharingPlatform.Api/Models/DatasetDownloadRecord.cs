using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatasetSharingPlatform.Api.Models
{
    public class DatasetDownloadRecord
    {
        [Key]
        public int Id { get; set; }  // 主键，自增

        [Required]
        public int UserId { get; set; }  // 下载的用户ID
        public User User { get; set; }

        [Required]
        public int DatasetId { get; set; }  // 被下载的数据集ID
        public Dataset Dataset { get; set; }

        [Required]
        public DateTime DownloadTime { get; set; }  // 下载时间


    }
}
