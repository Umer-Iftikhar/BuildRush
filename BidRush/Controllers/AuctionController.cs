using BidRush.DTOs.Request;
using BidRush.Services.Interfaces;
using BidRush.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BidRush.Controllers
{
    [Authorize]
    public class AuctionController : Controller
    {
        private readonly IAuctionService _auctionService;
        
        public AuctionController(IAuctionService auctionService)
        {
            _auctionService = auctionService;
        }

        #region Helpers
        private async Task LoadCategoriesAsync(CreateAuctionViewModel model)
        {
            model.Categories = await _auctionService.GetCategoriesAsync();
        }
        #endregion

        #region Create Auction
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateAuctionViewModel();

            await LoadCategoriesAsync(model);

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAuctionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategoriesAsync(model);
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var creatorId))
            {
                return Unauthorized();
            }

            var request = new CreateAuctionRequestDto
            {
                Title = model.Title,
                Description = model.Description,
                StartingPrice = model.StartingPrice,
                MinimumBidIncrement = model.MinimumBidIncrement,
                StartTime = model.StartTime!.Value,
                EndTime = model.EndTime!.Value,
                CategoryId = model.CategoryId
            };

            var result = await _auctionService.CreateAuctionAsync(request, creatorId, model.Image);

            if (result.ResponseCode != 200)
            {
                ModelState.AddModelError(string.Empty, result.ResponseMessage);

                await LoadCategoriesAsync(model);

                return View(model);
            }
            TempData["Success"] = result.ResponseMessage;
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        #endregion

        #region Details
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var auction = await _auctionService.GetAuctionAsync(id);

            if (auction.ResponseCode != 200)
            {
                if (auction.ResponseCode == 404)
                    return NotFound();

                return View("Error");
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isCreator = int.TryParse(userIdClaim, out var userId) && auction.CreatorId == userId;

            var model = new AuctionDetailsViewModel
            {
                Auction = auction,
                IsCreator = isCreator
            };

            return View(model);
        }
        #endregion

        #region Delete Auction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var creatorId))
            {
                return Json(new
                {
                    responseCode = 401,
                    responseMessage = "Unauthorized."
                });
            }

            var result = await _auctionService.DeleteAuctionAsync(id, creatorId);

            return Json(new
            {
                responseCode = result.ResponseCode,
                responseMessage = result.ResponseMessage
            });
        }
        #endregion

        #region Index
        [HttpGet]
        public async Task<IActionResult> Index(string? search)
        {
            var auctions = await _auctionService.GetAuctionsAsync(search);

            var model = new AuctionIndexViewModel
            {
                Search = search,
                Auctions = auctions
            };

            return View(model);
        }
        #endregion
    }
}
