using BidRush.DTOs.Response;
using System.ComponentModel.DataAnnotations;

namespace BidRush.ViewModels
{
    public class CreateAuctionViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }

        [Range(0, double.MaxValue)]
        public decimal StartingPrice { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Minimum bid increment must be greater than zero.")]
        public decimal MinimumBidIncrement { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public int? CategoryId { get; set; }

        public IEnumerable<CategoryResponseDto> Categories { get; set; } = Enumerable.Empty<CategoryResponseDto>();
    }
}
