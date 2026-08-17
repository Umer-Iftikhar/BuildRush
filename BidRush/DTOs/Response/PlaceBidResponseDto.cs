namespace BidRush.DTOs.Response
{
    public class PlaceBidResponseDto : SpResponseDto
    {
        public int AuctionId { get; set; }
        public int BidderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime EndTime { get; set; }
    }
}
