namespace BidRush.Constants
{
    public class StoredProcedures
    {
        // Users
        public const string CreateUser = "dbo.CreateUser";
        public const string GetUserByEmail = "dbo.GetUserByEmail";
        public const string GetUserById = "dbo.GetUserById";


        // Refresh Tokens
        public const string SaveRefreshToken = "dbo.SaveRefreshToken";
        public const string RotateRefreshToken = "dbo.RotateRefreshToken";
        public const string RevokeRefreshToken = "dbo.RevokeRefreshToken";
        public const string RevokeAllUserTokens = "dbo.RevokeAllUserTokens";

    }
}
