using BidRush.DTOs.Response;

namespace BidRush.ViewModels
{
    public class AuctionDetailsViewModel
    {
        public AuctionDto Auction { get; set; } = null!;
        public bool IsCreator { get; set; }
    }
}
