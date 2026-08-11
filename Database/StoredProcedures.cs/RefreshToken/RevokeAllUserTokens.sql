USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.RevokeAllUserTokens
(
    @UserId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        UPDATE dbo.RefreshTokens
        SET
            IsRevoked = 1
        WHERE UserId = @UserId;

        SELECT
            200 AS ResponseCode,
            'All Refresh Tokens Revoked Successfully' AS ResponseMessage;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END;
GO