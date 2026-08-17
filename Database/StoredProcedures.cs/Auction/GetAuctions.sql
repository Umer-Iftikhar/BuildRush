USE BidRushDb;
go

CREATE OR ALTER PROCEDURE dbo.GetAuctions
    @AuctionId INT = NULL,
    @Search NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        IF @AuctionId IS NOT NULL AND @AuctionId <= 0
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Invalid auction ID.' AS ResponseMessage;

            RETURN;
        END;

        IF @AuctionId IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM dbo.Auctions
               WHERE Id = @AuctionId
                 AND IsDeleted = 0
           )
        BEGIN
            SELECT
                404 AS ResponseCode,
                'Auction not found.' AS ResponseMessage;

            RETURN;
        END;

        SELECT
            200 AS ResponseCode,
            'Auctions retrieved successfully.' AS ResponseMessage;

        SELECT
            Id,
            Title,
            Description,
            ImageUrl,
            StartingPrice,
            MinimumBidIncrement,
            StartTime,
            EndTime,
            Status,
            IsDeleted,
            CreatorId,
            CategoryId,
            WinnerId,
            WinningBidAmount,
            CreatedAt
        FROM dbo.Auctions
        WHERE IsDeleted = 0
          AND (@AuctionId IS NULL OR Id = @AuctionId)
          AND
          (
              @Search IS NULL
              OR LTRIM(RTRIM(@Search)) = ''
              OR Title LIKE '%' + @Search + '%'
              OR Description LIKE '%' + @Search + '%'
              OR Id = TRY_CONVERT(INT, LTRIM(RTRIM(@Search)))
          )
        ORDER BY CreatedAt DESC;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            'An error occurred while retrieving auctions.' AS ResponseMessage;

    END CATCH
END;
GO