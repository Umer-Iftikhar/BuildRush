using BidRush.DTOs.Internal;

namespace BidRush.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(TokenClaimsDto claims);
    }
}
