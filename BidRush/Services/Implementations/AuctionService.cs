using BidRush.Constants;
using BidRush.Data;
using BidRush.DTOs;
using BidRush.DTOs.Request;
using BidRush.DTOs.Response;
using BidRush.Services.Interfaces;
using Dapper;
using System.Data;

namespace BidRush.Services.Implementations
{
    public class AuctionService : IAuctionService
    {
        private readonly DapperContext _context;
        private readonly IImageService _imageService;
        public AuctionService(DapperContext context, IImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        #region Create Auction
        public async Task<AuctionDto> CreateAuctionAsync(CreateAuctionRequestDto request, int creatorId, IFormFile? image)
        {
            if (request.StartTime <= DateTime.UtcNow)
            {
                return new AuctionDto
                {
                    ResponseCode = 400,
                    ResponseMessage = "Start time must be in the future."
                };
            }
            if (request.EndTime <= DateTime.UtcNow)
            {
                return new AuctionDto
                {
                    ResponseCode = 400,
                    ResponseMessage = "End time must be in the future."
                };
            }
            if (request.EndTime <= request.StartTime)
            {
                return new AuctionDto
                {
                    ResponseCode = 400,
                    ResponseMessage = "End time must be later than start time."
                };
            }

            if (image != null)
            {
                var imageResult =
                    await _imageService.SaveAuctionImageAsync(image);

                if (imageResult.ResponseCode != 200)
                {
                    return new AuctionDto
                    {
                        ResponseCode = imageResult.ResponseCode,
                        ResponseMessage = imageResult.ResponseMessage
                    };
                }

                request.ImageUrl = imageResult.FilePath;
            }

            using var connection = _context.CreateConnection();

            var parameters = new
            {
                request.Title,
                request.Description,
                request.ImageUrl,
                request.StartingPrice,
                request.MinimumBidIncrement,
                request.StartTime,  
                request.EndTime,
                CreatorId = creatorId,
                request.CategoryId
            };

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.CreateAuction,
                parameters,
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return new AuctionDto
                {
                    ResponseCode = response.ResponseCode,
                    ResponseMessage = response.ResponseMessage
                };
            }

            var auction = await multi.ReadSingleAsync<AuctionDto>();

            auction.ResponseCode = response.ResponseCode;
            auction.ResponseMessage = response.ResponseMessage;

            return auction;
        }
        #endregion

        #region Get Categories
        public async Task<IEnumerable<CategoryResponseDto>> GetCategoriesAsync()
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.GetCategories,
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
                return Enumerable.Empty<CategoryResponseDto>();

            return await multi.ReadAsync<CategoryResponseDto>();
        }
        #endregion

        #region Get Auction By Id
        public async Task<AuctionDto> GetAuctionAsync(int auctionId)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.GetAuctions,
                new { AuctionId = auctionId },
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return new AuctionDto
                {
                    ResponseCode = response.ResponseCode,
                    ResponseMessage = response.ResponseMessage
                };
            }

            var auction = await multi.ReadSingleAsync<AuctionDto>();

            auction.ResponseCode = response.ResponseCode;
            auction.ResponseMessage = response.ResponseMessage;

            if (!string.IsNullOrWhiteSpace(auction.ImageUrl))
            {
                var image = await _imageService.GetImageAsync(
                    auction.ImageUrl);

                if (image.ResponseCode != 200)
                {
                    auction.ImageUrl = null;
                }
                else
                {
                    auction.ImageUrl = image.FilePath;
                }
            }

            return auction;
        }
        #endregion

        #region Delete Auction
        public async Task<SpResponseDto> DeleteAuctionAsync(int auctionId, int creatorId)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.DeleteAuction,
                new
                {
                    AuctionId = auctionId,
                    CreatorId = creatorId
                },
                commandType: CommandType.StoredProcedure);

            return await multi.ReadSingleAsync<SpResponseDto>();
        }
        #endregion

        #region Get All Auctions
        public async Task<IEnumerable<AuctionDto>> GetAuctionsAsync(string? search)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.GetAuctions,
                new
                {
                    Search = search
                },
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return Enumerable.Empty<AuctionDto>();
            }

            var auctions = (await multi.ReadAsync<AuctionDto>()).ToList();

            foreach (var auction in auctions)
            {

                if (!string.IsNullOrWhiteSpace(auction.ImageUrl))
                {
                    var image = await _imageService.GetImageAsync(
                        auction.ImageUrl);

                    if (image.ResponseCode != 200)
                    {
                        auction.ImageUrl = null;
                    }
                    else
                    {
                        auction.ImageUrl = image.FilePath;
                    }
                }
            }

            return auctions;
        }
        #endregion

        #region Update Auction Statuses
        public async Task<IEnumerable<AuctionStatusUpdateDto>> UpdateAuctionStatusesAsync()
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.UpdateAuctionStatuses,
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return Enumerable.Empty<AuctionStatusUpdateDto>();
            }

            var updates = await multi.ReadAsync<AuctionStatusUpdateDto>();

            return updates.ToList();
        }
        #endregion
    }
}
