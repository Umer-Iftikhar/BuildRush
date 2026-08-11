namespace BidRush.DTOs.Response
{
    public class RegisterResponseDto : SpResponseDto
    {
        public int UserId { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}
