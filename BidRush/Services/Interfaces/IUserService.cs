using BidRush.DTOs;
using BidRush.DTOs.Request;
using BidRush.DTOs.Response;

namespace BidRush.Services.Interfaces
{
    public interface IUserService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task<SpResponseDto> LogoutAsync(string refreshToken);
    }
}
