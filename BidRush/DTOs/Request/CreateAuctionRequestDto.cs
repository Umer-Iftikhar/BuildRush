namespace BidRush.DTOs.Request
{
    public class CreateAuctionRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal StartingPrice { get; set; }
        public string? ImageUrl { get; set; }
        public decimal MinimumBidIncrement { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? CategoryId { get; set; }
    }
}
