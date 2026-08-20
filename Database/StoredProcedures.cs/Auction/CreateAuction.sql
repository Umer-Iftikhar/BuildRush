USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.CreateAuction
    @Title NVARCHAR(200),
    @Description NVARCHAR(2000) = NULL,
    @ImageUrl NVARCHAR(500) = NULL,
    @StartingPrice DECIMAL(18,2),
    @MinimumBidIncrement DECIMAL(18,2),
    @StartTime DATETIME2,
    @EndTime DATETIME2,
    @CreatorId INT,
    @CategoryId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        -- Validate creator
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Users
            WHERE Id = @CreatorId
              AND IsActive = 1
              AND IsDeleted = 0
        )
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Invalid creator.' AS ResponseMessage;

            RETURN;
        END;

        -- Validate title
        IF NULLIF(LTRIM(RTRIM(@Title)), '') IS NULL
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Auction title is required.' AS ResponseMessage;

            RETURN;
        END;

        -- Validate starting price
        IF @StartingPrice < 0
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Starting price cannot be negative.' AS ResponseMessage;

            RETURN;
        END;

        -- Validate minimum bid increment
        IF @MinimumBidIncrement <= 0
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Minimum bid increment must be greater than zero.' AS ResponseMessage;

            RETURN;
        END;

        -- Validate auction times
        IF @EndTime <= @StartTime
        BEGIN
            SELECT
                400 AS ResponseCode,
                'End time must be later than start time.' AS ResponseMessage;

            RETURN;
        END;

        -- Validate category when supplied
        IF @CategoryId IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM dbo.Categories
               WHERE Id = @CategoryId
           )
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Invalid category.' AS ResponseMessage;

            RETURN;
        END;

        BEGIN TRANSACTION;

        INSERT INTO dbo.Auctions
        (
            Title,
            Description,
            ImageUrl,
            StartingPrice,
            MinimumBidIncrement,
            StartTime,
            EndTime,
            CreatorId,
            CategoryId
        )
        VALUES
        (
            LTRIM(RTRIM(@Title)),
            @Description,
            @ImageUrl,
            @StartingPrice,
            @MinimumBidIncrement,
            @StartTime,
            @EndTime,
            @CreatorId,
            @CategoryId
        );

        DECLARE @AuctionId INT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT
            200 AS ResponseCode,
            'Auction created successfully.' AS ResponseMessage;

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
            CreatorId,
            CategoryId,
            WinnerId,
            WinningBidAmount,
            CreatedAt
        FROM dbo.Auctions
        WHERE Id = @AuctionId;

    END TRY
    BEGIN CATCH

        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END;
GO