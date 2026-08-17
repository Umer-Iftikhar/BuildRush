using BidRush.DTOs;
using BidRush.DTOs.Request;
using BidRush.DTOs.Response;

namespace BidRush.Services.Interfaces
{
    public interface IAuctionService
    {
        Task<AuctionDto> CreateAuctionAsync(CreateAuctionRequestDto request, int creatorId, IFormFile? image);
        Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync();
        Task<AuctionDto> GetAuctionAsync(int auctionId);
        Task<SpResponseDto> DeleteAuctionAsync(int auctionId, int creatorId);
        Task<IEnumerable<AuctionDto>> GetAuctionsAsync(string? search);
    }
}
