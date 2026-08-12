namespace BidRush.Models
{
    public class Auction
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal StartingPrice { get; set; }
        public decimal MinimumBidIncrement { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public int CreatorId { get; set; }
        public int? CategoryId { get; set; }
        public int? WinnerId { get; set; }
        public decimal? WinningBidAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
