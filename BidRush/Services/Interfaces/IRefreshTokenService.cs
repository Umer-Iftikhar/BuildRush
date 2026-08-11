using BidRush.DTOs;
using BidRush.DTOs.Response;

namespace BidRush.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string?> GenerateAndSaveAsync(int userId);
        Task<LoginResponseDto> RefreshAsync(string refreshToken);
        Task<SpResponseDto> RevokeAsync(string refreshToken);
    }
}
