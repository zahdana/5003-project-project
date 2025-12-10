// DatasetUploadDto.cs
namespace DatasetSharingPlatform.Api.DTOs
{
    public class DatasetUploadDto
    {
        // 数据集的名称
        public string Name { get; set; } = string.Empty;

        // 数据集的描述
        public string Description { get; set; } = string.Empty;

        // 数据集的类型
        public string Type { get; set; } = string.Empty;

        // 数据集的下载权限，默认是公开
        public string DownloadPermission { get; set; } = "公开";

        // 文件上传，接收前端文件
        public required IFormFile File { get; set; }

        //标签 ID 列表，接收前端多选的标签
        public List<int> TagIds { get; set; } = new List<int>();
    }
}
