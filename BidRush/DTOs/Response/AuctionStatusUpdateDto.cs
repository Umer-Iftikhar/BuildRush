namespace BidRush.DTOs.Response
{
    public class AuctionStatusUpdateDto
    {
        public int AuctionId { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
    }
}
