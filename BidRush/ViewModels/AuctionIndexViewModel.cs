using BidRush.DTOs.Response;

namespace BidRush.ViewModels
{
    public class AuctionIndexViewModel
    {
        public string? Search { get; set; }
        public IEnumerable<AuctionDto> Auctions { get; set; } = Enumerable.Empty<AuctionDto>();
    }
}
