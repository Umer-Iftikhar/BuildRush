USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.RevokeRefreshToken
(
    @Token NVARCHAR(500)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        BEGIN TRANSACTION;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.RefreshTokens
            WHERE Token = @Token
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                404 AS ResponseCode,
                'Refresh Token Not Found' AS ResponseMessage;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RefreshTokens
            WHERE Token = @Token
              AND IsRevoked = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                200 AS ResponseCode,
                'Refresh Token Already Revoked' AS ResponseMessage;

            RETURN;
        END;

        UPDATE dbo.RefreshTokens
        SET IsRevoked = 1
        WHERE Token = @Token;

        COMMIT TRANSACTION;

        SELECT
            200 AS ResponseCode,
            'Refresh Token Revoked Successfully' AS ResponseMessage;

    END TRY
    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END;
GO