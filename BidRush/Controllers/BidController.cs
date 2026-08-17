using BidRush.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidRush.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly IBidService _bidService;
        public BidController(IBidService bidService)
        {
            _bidService = bidService;
        }
        #region Place Bid
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int auctionId, decimal amount)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var bidderId))
            {
                return Json(new
                {
                    responseCode = 401,
                    responseMessage = "Unauthorized."
                });
            }

            var result = await _bidService.PlaceBidAsync(auctionId, bidderId, amount);

            return Json(new
            {
                responseCode = result.ResponseCode,
                responseMessage = result.ResponseMessage,
                auctionId = result.AuctionId,
                bidderId = result.BidderId,
                amount = result.Amount,
                endTime = result.EndTime
            });
        }
        #endregion
    }
}
