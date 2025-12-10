using System.Security.Claims;

namespace DatasetSharingPlatform.Api.Helpers
{
    public static class JwtHelper
    {
        public static int GetUserId(ClaimsPrincipal user)
        {
            // 支持多种 claim 类型，兼容不同 token 格式
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId") ?? user.FindFirst("id");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("无法从Token中获取用户ID");
        }
    }
}
