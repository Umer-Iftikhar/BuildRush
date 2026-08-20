USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.PlaceBid
    @AuctionId INT,
    @BidderId INT,
    @Amount DECIMAL(18,2)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE
        @Status NVARCHAR(20),
        @EndTime DATETIME2,
        @StartingPrice DECIMAL(18,2),
        @MinimumBidIncrement DECIMAL(18,2),
        @CreatorId INT,
        @CurrentHighestBid DECIMAL(18,2),
        @RequiredBid DECIMAL(18,2),
        @Now DATETIME2 = SYSUTCDATETIME();

    BEGIN TRY

        BEGIN TRANSACTION;

        /*
            Lock the auction row.

            This is the concurrency control mechanism.
            Concurrent bids for the same auction must wait here
            until the current transaction commits or rolls back.
        */
        SELECT
            @Status = Status,
            @EndTime = EndTime,
            @StartingPrice = StartingPrice,
            @MinimumBidIncrement = MinimumBidIncrement,
            @CreatorId = CreatorId
        FROM dbo.Auctions WITH (UPDLOCK, ROWLOCK)
        WHERE Id = @AuctionId
          AND IsDeleted = 0;

        -- Auction must exist and not be deleted
        IF @Status IS NULL
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                404 AS ResponseCode,
                'Auction not found.' AS ResponseMessage;

            RETURN;
        END;

        -- Bidder must exist and be active
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Users
            WHERE Id = @BidderId
              AND IsActive = 1
              AND IsDeleted = 0
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                401 AS ResponseCode,
                'Bidder is not authorized.' AS ResponseMessage;

            RETURN;
        END;

        -- Creator cannot bid on their own auction
        IF @CreatorId = @BidderId
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                403 AS ResponseCode,
                'You cannot bid on your own auction.' AS ResponseMessage;

            RETURN;
        END;

        -- Auction must be active
        IF @Status <> 'Active'
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                400 AS ResponseCode,
                'Bids can only be placed on active auctions.' AS ResponseMessage;

            RETURN;
        END;

        -- Auction must not have ended
        IF @Now >= @EndTime
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                400 AS ResponseCode,
                'This auction has ended.' AS ResponseMessage;

            RETURN;
        END;

        -- Bid amount must be positive
        IF @Amount <= 0
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                400 AS ResponseCode,
                'Bid amount must be greater than zero.' AS ResponseMessage;

            RETURN;
        END;

        /*
            Because the auction row is locked above, another bid
            for this auction cannot modify the auction state while
            we determine and validate the current highest bid.
        */
        SELECT
            @CurrentHighestBid = MAX(Amount)
        FROM dbo.Bids
        WHERE AuctionId = @AuctionId;

        -- Determine minimum acceptable bid
        SET @RequiredBid =
            CASE
                WHEN @CurrentHighestBid IS NULL
                    THEN @StartingPrice + @MinimumBidIncrement
                ELSE @CurrentHighestBid + @MinimumBidIncrement
            END;

        -- Bid must satisfy minimum increment
        IF @Amount < @RequiredBid
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                400 AS ResponseCode,
                CONCAT(
                    'Bid must be at least ',
                    FORMAT(@RequiredBid, '0.00'),
                    '.'
                ) AS ResponseMessage;

            RETURN;
        END;

        -- Insert bid
        INSERT INTO dbo.Bids
        (
            AuctionId,
            BidderId,
            Amount
        )
        VALUES
        (
            @AuctionId,
            @BidderId,
            @Amount
        );

        /*
            Auto-extension:
            If a valid bid is placed during the final 30 seconds,
            extend the auction by 30 seconds.
        */
        IF @Now >= DATEADD(SECOND, -30, @EndTime)
        BEGIN
            SET @EndTime = DATEADD(SECOND, 30, @EndTime);

            UPDATE dbo.Auctions
            SET EndTime = @EndTime
            WHERE Id = @AuctionId;
        END;

        COMMIT TRANSACTION;

        /*
            Return operation result followed by the updated
            bidding state for the caller / SignalR layer.
        */
        SELECT
            200 AS ResponseCode,
            'Bid placed successfully.' AS ResponseMessage;

        SELECT
            @AuctionId AS AuctionId,
            @BidderId AS BidderId,
            @Amount AS Amount,
            @EndTime AS EndTime;

    END TRY
    BEGIN CATCH

        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        SELECT
            500 AS ResponseCode,
            'An error occurred while placing the bid.' AS ResponseMessage;

    END CATCH
END;
GO