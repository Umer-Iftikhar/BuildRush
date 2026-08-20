using BidRush.Constants;
using BidRush.Data;
using BidRush.DTOs;
using BidRush.DTOs.Response;
using BidRush.Services.Interfaces;
using Dapper;
using System.Data;

namespace BidRush.Services.Implementations
{
    public class BidService : IBidService
    {
        private readonly DapperContext _context;
        public BidService(DapperContext context)
        {
            _context = context;
        }

        #region Place Bid
        public async Task<PlaceBidResponseDto> PlaceBidAsync(int auctionId, int bidderId, decimal amount)
        {
            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.PlaceBid,
                new
                {
                    AuctionId = auctionId,
                    BidderId = bidderId,
                    Amount = amount
                },
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return new PlaceBidResponseDto
                {
                    ResponseCode = response.ResponseCode,
                    ResponseMessage = response.ResponseMessage
                };
            }

            var result = await multi.ReadSingleAsync<PlaceBidResponseDto>();

            result.ResponseCode = response.ResponseCode;
            result.ResponseMessage = response.ResponseMessage;

            return result;
        }
        #endregion
    }
}
