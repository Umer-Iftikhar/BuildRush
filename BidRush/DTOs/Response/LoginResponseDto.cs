namespace BidRush.DTOs.Response
{
    public class LoginResponseDto : SpResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
