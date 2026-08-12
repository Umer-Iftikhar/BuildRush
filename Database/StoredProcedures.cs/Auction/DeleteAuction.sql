USE BidRushDb;
go

CREATE OR ALTER PROCEDURE dbo.DeleteAuction
    @AuctionId INT,
    @CreatorId INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        -- Auction must exist
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Auctions
            WHERE Id = @AuctionId
        )
        BEGIN
            SELECT
                404 AS ResponseCode,
                'Auction not found.' AS ResponseMessage;
            RETURN;
        END;

        -- Auction must not already be deleted
        IF EXISTS
        (
            SELECT 1
            FROM dbo.Auctions
            WHERE Id = @AuctionId
              AND IsDeleted = 1
        )
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Auction has already been deleted.' AS ResponseMessage;
            RETURN;
        END;

        -- Only the creator can delete the auction
        IF EXISTS
        (
            SELECT 1
            FROM dbo.Auctions
            WHERE Id = @AuctionId
              AND CreatorId <> @CreatorId
        )
        BEGIN
            SELECT
                403 AS ResponseCode,
                'You are not authorized to delete this auction.' AS ResponseMessage;
            RETURN;
        END;

        -- Only pending auctions can be deleted
        IF EXISTS
        (
            SELECT 1
            FROM dbo.Auctions
            WHERE Id = @AuctionId
              AND Status <> 'Pending'
        )
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Only pending auctions can be deleted.' AS ResponseMessage;
            RETURN;
        END;

        -- Soft delete
        UPDATE dbo.Auctions
        SET IsDeleted = 1
        WHERE Id = @AuctionId
          AND CreatorId = @CreatorId
          AND Status = 'Pending'
          AND IsDeleted = 0;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT
                400 AS ResponseCode,
                'Auction could not be deleted.' AS ResponseMessage;
            RETURN;
        END;

        SELECT
            200 AS ResponseCode,
            'Auction deleted successfully.' AS ResponseMessage;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            'An error occurred while deleting the auction.' AS ResponseMessage;

    END CATCH
END;
GO