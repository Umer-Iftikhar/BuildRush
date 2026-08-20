USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.GetCategories
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        SELECT
            200 AS ResponseCode,
            'Categories retrieved successfully.' AS ResponseMessage;

        SELECT
            Id,
            Name,
            CreatedAt
        FROM dbo.Categories
        ORDER BY Name;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END;
GO