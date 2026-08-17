using BidRush.DTOs.Response;

namespace BidRush.Services.Interfaces
{
    public interface IBidService
    {
        Task<PlaceBidResponseDto> PlaceBidAsync(int auctionId, int bidderId, decimal amount);
    }
}
