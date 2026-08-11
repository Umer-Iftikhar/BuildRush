USE BidRushDb;
GO

CREATE OR ALTER PROCEDURE dbo.CreateUser
(
    @Name NVARCHAR(100),
    @Email NVARCHAR(250),
    @PasswordHash NVARCHAR(300)
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Users
            WHERE Email = @Email
        )
        BEGIN
            SELECT
                409 AS ResponseCode,
                'Email already exists.' AS ResponseMessage;

            RETURN;
        END;

        DECLARE @UserRoleId INT;

        SELECT @UserRoleId = Id
        FROM dbo.Roles
        WHERE Name = 'User';

        IF @UserRoleId IS NULL
        BEGIN
            SELECT
                500 AS ResponseCode,
                'User role not found.' AS ResponseMessage;

            RETURN;
        END;

        INSERT INTO dbo.Users
        (
            Name,
            Email,
            PasswordHash,
            RoleId
        )
        VALUES
        (
            @Name,
            @Email,
            @PasswordHash,
            @UserRoleId
        );

        DECLARE @UserId INT = CAST(SCOPE_IDENTITY() AS INT);

        SELECT
            200 AS ResponseCode,
            'User created successfully.' AS ResponseMessage,
            @UserId AS UserId,
            'User' AS RoleName;

    END TRY
    BEGIN CATCH

        SELECT
            500 AS ResponseCode,
            ERROR_MESSAGE() AS ResponseMessage;

    END CATCH
END;
GO