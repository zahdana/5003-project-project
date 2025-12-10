namespace DatasetSharingPlatform.Api.DTOs
{
    public class PermissionRequestDto
    {
        public int DatasetId { get; set; }
        // 后端可通过 JWT 获取 RequestedUserId，此字段可选
        public string? Message { get; set; }  // 可选备注字段
    }
}
