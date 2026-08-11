using BidRush.Constants;
using BidRush.Services.Interfaces;
using BidRush.Settings;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace BidRush.Middlewares
{
    public class RefreshTokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtConfig _jwtConfig;

        public RefreshTokenMiddleware(RequestDelegate next, IOptions<JwtConfig> options)
        {
            _next = next;
            _jwtConfig = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IRefreshTokenService refreshTokenService)
        {
            var hasAccessToken = context.Request.Cookies.TryGetValue(CookieConstants.AccessToken, out var accessToken);
            var hasRefreshToken = context.Request.Cookies.TryGetValue(CookieConstants.RefreshToken, out var refreshToken);

            var needsRefresh = hasRefreshToken && (!hasAccessToken || IsExpired(accessToken!));

            if (needsRefresh)
            {
                var response = await refreshTokenService.RefreshAsync(refreshToken!);

                if (response.ResponseCode == 200)
                {
                    context.Items[CookieConstants.AccessToken] = response.AccessToken;

                    context.Response.Cookies.Append(CookieConstants.AccessToken, response.AccessToken!,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.ExpiryMinutes)
                        });

                    context.Response.Cookies.Append(CookieConstants.RefreshToken, response.RefreshToken!,
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(_jwtConfig.RefreshTokenExpiryDays)
                        });
                }
                else
                {
                    context.Response.Cookies.Delete(CookieConstants.AccessToken);
                    context.Response.Cookies.Delete(CookieConstants.RefreshToken);
                }
            }

            await _next(context);
        }

        private static bool IsExpired(string jwt)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(jwt))
                    return false;

                var token = handler.ReadJwtToken(jwt);

                return token.ValidTo <= DateTime.UtcNow.AddSeconds(30);
            }
            catch
            {
                return false;
            }
        }
    }
}
