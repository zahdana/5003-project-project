namespace DatasetSharingPlatform.Api.Services
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public string Token { get; set; } = "";
        public bool IsAdmin { get; set; } = false;
    }
}
