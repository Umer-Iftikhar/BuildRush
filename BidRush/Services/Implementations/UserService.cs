using BidRush.Constants;
using BidRush.Data;
using BidRush.DTOs;
using BidRush.DTOs.Internal;
using BidRush.DTOs.Request;
using BidRush.DTOs.Response;
using BidRush.Services.Interfaces;
using BidRush.Settings;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;

namespace BidRush.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly ITokenService _tokenService;
        private readonly DapperContext _context;
        private readonly IRefreshTokenService _refreshTokenService;

        public UserService(IOptions<JwtConfig> options, ITokenService tokenService, DapperContext context, IRefreshTokenService refreshTokenService)
        {
            _jwtConfig = options.Value;
            _tokenService = tokenService;
            _context = context;
            _refreshTokenService = refreshTokenService;
        }

        # region Register
        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            using var connection = _context.CreateConnection();

            var userResponse = await connection.QuerySingleAsync<RegisterResponseDto>(
                StoredProcedures.CreateUser,
                new
                {
                    Name = request.Name,
                    Email = request.Email,
                    PasswordHash = passwordHash
                },
                commandType: CommandType.StoredProcedure);

            if (userResponse.ResponseCode != 200)
            {
                return new LoginResponseDto
                {
                    ResponseCode = userResponse.ResponseCode,
                    ResponseMessage = userResponse.ResponseMessage
                };
            }

            var refreshToken = await _refreshTokenService.GenerateAndSaveAsync(userResponse.UserId);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new LoginResponseDto
                {
                    ResponseCode = 500,
                    ResponseMessage = "Failed To Generate Refresh Token"
                };
            }

            var claims = new TokenClaimsDto
            {
                Id = userResponse.UserId,
                Name = request.Name,
                Email = request.Email,
                Role = userResponse.RoleName
            };

            var accessToken = _tokenService.GenerateToken(claims);

            return new LoginResponseDto
            {
                ResponseCode = 200,
                ResponseMessage = "Registration Successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
        #endregion

        #region Login
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            using var connection = _context.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                StoredProcedures.GetUserByEmail,
                new { Email = request.Email },
                commandType: CommandType.StoredProcedure);

            var response = await multi.ReadSingleAsync<SpResponseDto>();

            if (response.ResponseCode != 200)
            {
                return new LoginResponseDto
                {
                    ResponseCode = 401,
                    ResponseMessage = "Invalid Email Or Password"
                };
            }

            var user = await multi.ReadSingleOrDefaultAsync<UserDto>();

            if (user is null)
            {
                return new LoginResponseDto
                {
                    ResponseCode = 401,
                    ResponseMessage = "Invalid Email Or Password"
                };
            }

            if (!user.IsActive || user.IsDeleted)
            {
                return new LoginResponseDto
                {
                    ResponseCode = 401,
                    ResponseMessage = "Account Is Not Active"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new LoginResponseDto
                {
                    ResponseCode = 401,
                    ResponseMessage = "Invalid Email Or Password"
                };
            }

            var refreshToken = await _refreshTokenService.GenerateAndSaveAsync(user.Id);

            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return new LoginResponseDto
                {
                    ResponseCode = 500,
                    ResponseMessage = "Failed To Generate Refresh Token"
                };
            }

            var claims = new TokenClaimsDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.RoleName,
            };

            var accessToken = _tokenService.GenerateToken(claims);

            return new LoginResponseDto
            {
                ResponseCode = 200,
                ResponseMessage = "Login Successful",
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        #endregion

        #region Logout
        public async Task<SpResponseDto> LogoutAsync(string refreshToken)
        {
            return await _refreshTokenService.RevokeAsync(refreshToken);
        }
        #endregion
    }
}
