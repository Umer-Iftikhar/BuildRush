using BidRush.DTOs.Response;

namespace BidRush.Services.Interfaces
{
    public interface IImageService
    {
        Task<ImagePathResponseDto> GetImageAsync(string? relativePath);
        Task<ImagePathResponseDto> SaveAuctionImageAsync(IFormFile image);
    }
}
