USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.SaveRefreshToken
(
    @UserId INT,
    @Token NVARCHAR(500),
    @ExpiresAt DATETIME2
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        INSERT INTO dbo.RefreshTokens
        (
            UserId,
            Token,
            ExpiresAt
        )
        VALUES
        (
            @UserId,
            @Token,
            @ExpiresAt
        );


        SELECT
            200 AS ResponseCode,
                'Refresh Token Saved Successfully' AS ResponseMessage,
            
            CAST(SCOPE_IDENTITY() AS INT) AS RefreshTokenId;


    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END
GO