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
            a.Id,
            a.Title,
            a.Description,
            a.ImageUrl,
            a.StartingPrice,
            a.MinimumBidIncrement,
            a.StartTime,
            a.EndTime,
            a.Status,
            a.IsDeleted,
            a.CreatorId,
            a.CategoryId,
            a.WinnerId,
            a.WinningBidAmount,
            a.CreatedAt,

            MAX(b.Amount) AS CurrentHighestBid,
            COUNT(b.Id) AS BidCount

        FROM dbo.Auctions a
        LEFT JOIN dbo.Bids b
          ON b.AuctionId = a.Id
        WHERE a.IsDeleted = 0
          AND (@AuctionId IS NULL OR a.Id = @AuctionId)
          AND
          (
              @Search IS NULL
              OR LTRIM(RTRIM(@Search)) = ''
              OR a.Title LIKE '%' + @Search + '%'
              OR a.Description LIKE '%' + @Search + '%'
              OR a.Id = TRY_CONVERT(INT, LTRIM(RTRIM(@Search)))
          )
         GROUP BY
            a.Id,
            a.Title,
            a.Description,
            a.ImageUrl,
            a.StartingPrice,
            a.MinimumBidIncrement,
            a.StartTime,
            a.EndTime,
            a.Status,
            a.IsDeleted,
            a.CreatorId,
            a.CategoryId,
            a.WinnerId,
            a.WinningBidAmount,
            a.CreatedAt

        ORDER BY CreatedAt DESC;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            'An error occurred while retrieving auctions.' AS ResponseMessage;

    END CATCH
END;
GO