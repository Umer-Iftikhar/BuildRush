using BidRush.Hubs;
using BidRush.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace BidRush.Controllers
{
    [Authorize]
    public class BidController : Controller
    {
        private readonly IBidService _bidService;
        private readonly IHubContext<AuctionHub> _hubContext;
        public BidController(IBidService bidService, IHubContext<AuctionHub> hubContext)
        {
            _bidService = bidService;
            _hubContext = hubContext;
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

            if (result.ResponseCode != 200)
            {
                return Json(new
                {
                    responseCode = result.ResponseCode,
                    responseMessage = result.ResponseMessage
                });
            }

            await _hubContext.Clients.Group($"auction_{auctionId}").SendAsync("BidPlaced", result);

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
