USE BidRushDb;
GO
CREATE OR ALTER PROCEDURE dbo.UpdateAuctionStatuses
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Now DATETIME2 = SYSUTCDATETIME();
        DECLARE @UpdatedAuctions TABLE
        (
            AuctionId INT,
            PreviousStatus NVARCHAR(50),
            NewStatus NVARCHAR(50)
        );
        -- Pending → Active
        UPDATE dbo.Auctions
        SET Status = 'Active'
        OUTPUT
            inserted.Id,
            deleted.Status,
            inserted.Status
        INTO @UpdatedAuctions
        (
            AuctionId,
            PreviousStatus,
            NewStatus
        )
        WHERE Status = 'Pending'
          AND IsDeleted = 0
          AND StartTime <= @Now;
        -- Active → Ended
        ;WITH EndedAuctions AS
        (
            SELECT
                a.Id,
                HighestBid.BidderId,
                HighestBid.Amount
            FROM dbo.Auctions a

            OUTER APPLY
            (
                SELECT TOP 1
                    b.BidderId,
                    b.Amount
                FROM dbo.Bids b
                WHERE b.AuctionId = a.Id
                ORDER BY
                    b.Amount DESC,
                    b.PlacedAt ASC,
                    b.Id ASC
            ) HighestBid

            WHERE a.Status = 'Active'
            AND a.IsDeleted = 0
            AND a.EndTime <= @Now
        )
        UPDATE a
        SET
            Status = 'Ended',
            WinnerId = ea.BidderId,
            WinningBidAmount = ea.Amount
        OUTPUT
            inserted.Id,
            deleted.Status,
            inserted.Status
        INTO @UpdatedAuctions
        (
            AuctionId,
            PreviousStatus,
            NewStatus
        )
        FROM dbo.Auctions a
        INNER JOIN EndedAuctions ea
            ON ea.Id = a.Id;
        COMMIT TRANSACTION;
        SELECT
            200 AS ResponseCode,
            'Auction statuses updated successfully.' AS ResponseMessage;
        SELECT
            AuctionId,
            PreviousStatus,
            NewStatus
        FROM @UpdatedAuctions
        ORDER BY AuctionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT
            500 AS ResponseCode,
            'An error occurred while updating auction statuses.' AS ResponseMessage;
    END CATCH
END;
GO