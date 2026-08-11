USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.RotateRefreshToken
(
    @OldToken NVARCHAR(500),
    @NewToken NVARCHAR(500),
    @NewExpiresAt DATETIME2
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
            WHERE Token = @OldToken
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                404 AS ResponseCode,
                'Refresh Token Not Found' AS ResponseMessage;

            SELECT
                u.Id,
                u.Name,
                u.Email,
                r.Name AS RoleName
            FROM dbo.Users AS u
            INNER JOIN dbo.Roles AS r
                ON r.Id = u.RoleId
            WHERE 1 = 0;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RefreshTokens
            WHERE Token = @OldToken
              AND IsRevoked = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                401 AS ResponseCode,
                'Refresh Token Has Been Revoked' AS ResponseMessage;

            SELECT
                u.Id,
                u.Name,
                u.Email,
                r.Name AS RoleName
            FROM dbo.Users AS u
            INNER JOIN dbo.Roles AS r
                ON r.Id = u.RoleId
            WHERE 1 = 0;

            RETURN;
        END;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.RefreshTokens
            WHERE Token = @OldToken
              AND ExpiresAt <= SYSUTCDATETIME()
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                401 AS ResponseCode,
                'Refresh Token Has Expired' AS ResponseMessage;

            SELECT
                u.Id,
                u.Name,
                u.Email,
                r.Name AS RoleName
            FROM dbo.Users AS u
            INNER JOIN dbo.Roles AS r
                ON r.Id = u.RoleId
            WHERE 1 = 0;

            RETURN;
        END;

        DECLARE @UserId INT;

        SELECT @UserId = UserId
        FROM dbo.RefreshTokens
        WHERE Token = @OldToken;

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Users
            WHERE Id = @UserId
              AND IsDeleted = 1
        )
        BEGIN
            ROLLBACK TRANSACTION;

            SELECT
                401 AS ResponseCode,
                'User Account Is Deleted' AS ResponseMessage;

            SELECT
                u.Id,
                u.Name,
                u.Email,
                r.Name AS RoleName
            FROM dbo.Users AS u
            INNER JOIN dbo.Roles AS r
                ON r.Id = u.RoleId
            WHERE 1 = 0;

            RETURN;
        END;

        UPDATE dbo.RefreshTokens
        SET IsRevoked = 1
        WHERE Token = @OldToken;

        INSERT INTO dbo.RefreshTokens
        (
            UserId,
            Token,
            ExpiresAt
        )
        VALUES
        (
            @UserId,
            @NewToken,
            @NewExpiresAt
        );

        COMMIT TRANSACTION;

        SELECT
            200 AS ResponseCode,
            'Refresh Token Rotated Successfully' AS ResponseMessage;

        SELECT
            u.Id,
            u.Name,
            u.Email,
            r.Name AS RoleName
        FROM dbo.Users AS u
        INNER JOIN dbo.Roles AS r
            ON r.Id = u.RoleId
        WHERE u.Id = @UserId;

    END TRY
    BEGIN CATCH

        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

        SELECT
            u.Id,
            u.Name,
            u.Email,
            r.Name AS RoleName
        FROM dbo.Users AS u
        INNER JOIN dbo.Roles AS r
            ON r.Id = u.RoleId
        WHERE 1 = 0;

    END CATCH
END;
GO