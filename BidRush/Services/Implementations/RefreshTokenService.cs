using BidRush.Constants;
using BidRush.Data;
using BidRush.DTOs;
using BidRush.DTOs.Internal;
using BidRush.DTOs.Response;
using BidRush.Services.Interfaces;
using BidRush.Settings;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Cryptography;

namespace BidRush.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly DapperContext _context;
        private readonly ITokenService _tokenService;
        private readonly JwtConfig _jwtConfig;

        public RefreshTokenService(DapperContext context, ITokenService tokenService, IOptions<JwtConfig> jwtConfig)
        {
            _context = context;
            _tokenService = tokenService;
            _jwtConfig = jwtConfig.Value;
        }
        #region Helper Method
        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }
        #endregion

        #region Generate and Save Refresh Token
        public async Task<string?> GenerateAndSaveAsync(int userId)
        {
            var refreshToken = GenerateRefreshToken();

            var expiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays);
            using var connection = _context.CreateConnection();

            var response = await connection.QuerySingleAsync<SpResponseDto>(
                StoredProcedures.SaveRefreshToken,
                new
                {
                    UserId = userId,
                    Token = refreshToken,
                    ExpiresAt = expiresAt
                },
                commandType: CommandType.StoredProcedure);

            if (response.ResponseCode != 200)
            {
                return null;
            }

            return refreshToken;
        }
        #endregion

        #region Refresh Token
        public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
        {
            var newRefreshToken = GenerateRefreshToken();
            var newExpiresAt = DateTime.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays);

            using var connection = _context.CreateConnection();

            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.RotateRefreshToken,
                new
                {
                    OldToken = refreshToken,
                    NewToken = newRefreshToken,
                    NewExpiresAt = newExpiresAt
                },
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return new LoginResponseDto
                {
                    ResponseCode = response.ResponseCode,
                    ResponseMessage = response.ResponseMessage
                };
            }

            var user = await multi.ReadSingleOrDefaultAsync<UserDto>();

            if (user is null)
            {
                return new LoginResponseDto
                {
                    ResponseCode = 500,
                    ResponseMessage = "Failed To Retrieve User Information"
                };
            }
            var claims = new TokenClaimsDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.RoleName
            };

            var accessToken = _tokenService.GenerateToken(claims);
            return new LoginResponseDto
            {
                ResponseCode = 200,
                ResponseMessage = "Token Refreshed Successfully",
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }
        #endregion

        #region Revoke Refresh Token
        public async Task<SpResponseDto> RevokeAsync(string refreshToken)
        {
            using var connection = _context.CreateConnection();

            return await connection.QuerySingleAsync<SpResponseDto>(
                StoredProcedures.RevokeRefreshToken,
                new { Token = refreshToken },
                commandType: CommandType.StoredProcedure);

        }
        #endregion
    }
}
