USE BidRushDb;
GO

IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.Roles
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		Name NVARCHAR(50) NOT NULL,

		CONSTRAINT UQ_Roles_Name UNIQUE (Name)
	);
END
GO


IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.Users
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		Name NVARCHAR(100) NOT NULL,
		Email NVARCHAR(250) NOT NULL,
		PasswordHash NVARCHAR(300) NOT NULL,

		IsActive BIT NOT NULL
			CONSTRAINT DF_Users_IsActive DEFAULT(1),

		IsDeleted BIT NOT NULL
			Constraint DF_Users_IsDeleted DEFAULT (0),

		CreatedAt DATETIME2 NOT NULL
			CONSTRAINT DF_Users_CreatedAt
				DEFAULT(SYSUTCDATETIME()),

		RoleId INT NOT NULL
			CONSTRAINT FK_Users_Roles
				FOREIGN KEY REFERENCES dbo.Roles(Id)
	);

	CREATE UNIQUE INDEX IX_Users_Email 
		ON dbo.Users(Email);
END
GO


IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.RefreshTokens
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		UserId INT NOT NULL,
		Token NVARCHAR(500) NOT NULL,
		ExpiresAt DATETIME2 NOT NULL,

		CreatedAt DATETIME2 NOT NULL
			CONSTRAINT DF_RefreshTokens_CreatedAt
				DEFAULT (SYSUTCDATETIME()),

		IsRevoked BIT NOT NULL
			CONSTRAINT DF_RefreshTokens_IsRevoked
				DEFAULT (0),

		CONSTRAINT FK_RefreshTokens_Users
			FOREIGN KEY (UserId) REFERENCES dbo.Users(Id),

		CONSTRAINT UQ_RefreshTokens_Token
			UNIQUE (Token)
	);

	CREATE INDEX IX_RefreshTokens_UserId
		ON dbo.RefreshTokens(UserId);
END
GO


IF OBJECT_ID('dbo.Categories', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,

        CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Categories_CreatedAt
                DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT UQ_Categories_Name
            UNIQUE (Name)
    );
END
GO


IF OBJECT_ID('dbo.Auctions', 'U') IS NULL
BEGIN
	CREATE TABLE dbo.Auctions
    (
		Id INT IDENTITY(1,1) PRIMARY KEY,
		Title NVARCHAR(200) NOT NULL,
		Description NVARCHAR(2000) NULL,
		ImageUrl NVARCHAR(500) NULL,
		StartingPrice DECIMAL(18,2) NOT NULL,
		MinimumBidIncrement DECIMAL(18,2) NOT NULL,
		StartTime DATETIME2 NOT NULL,
		EndTime DATETIME2 NOT NULL,

		Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Auctions_Status
                DEFAULT ('Pending'),

		CreatorId INT NOT NULL,
		CategoryId INT NULL,
		WinnerId INT NULL,
		WinningBidAmount DECIMAL(18,2) NULL,

		CreatedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Auctions_CreatedAt
                DEFAULT (SYSUTCDATETIME()),

		CONSTRAINT FK_Auctions_Creator
            FOREIGN KEY (CreatorId)
				REFERENCES dbo.Users(Id),

		CONSTRAINT FK_Auctions_Category
            FOREIGN KEY (CategoryId)
				REFERENCES dbo.Categories(Id),

		CONSTRAINT FK_Auctions_Winner
            FOREIGN KEY (WinnerId)
				REFERENCES dbo.Users(Id),

		CONSTRAINT CK_Auctions_Status
            CHECK (Status IN ('Pending', 'Active', 'Ended')),

		CONSTRAINT CK_Auctions_Times
            CHECK (EndTime > StartTime),

		CONSTRAINT CK_Auctions_Prices
            CHECK
			(
                StartingPrice >= 0
                AND MinimumBidIncrement > 0
            )
	);

	CREATE INDEX IX_Auctions_CreatorId
        ON dbo.Auctions(CreatorId);

    CREATE INDEX IX_Auctions_Status_EndTime
        ON dbo.Auctions(Status, EndTime);

END
GO


IF OBJECT_ID('dbo.Bids', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bids
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        AuctionId INT NOT NULL,
        BidderId INT NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,

        PlacedAt DATETIME2 NOT NULL
            CONSTRAINT DF_Bids_PlacedAt
                DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT FK_Bids_Auctions
            FOREIGN KEY (AuctionId)
				REFERENCES dbo.Auctions(Id),

        CONSTRAINT FK_Bids_Users
            FOREIGN KEY (BidderId)
				REFERENCES dbo.Users(Id),

        CONSTRAINT CK_Bids_Amount
            CHECK (Amount > 0)
    );

    CREATE INDEX IX_Bids_AuctionId
        ON dbo.Bids(AuctionId);

    CREATE INDEX IX_Bids_BidderId
        ON dbo.Bids(BidderId);
END
GO


INSERT INTO dbo.Roles (Name)
SELECT 'Admin'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE Name = 'Admin'
);

INSERT INTO dbo.Roles (Name)
SELECT 'User'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Roles
    WHERE Name = 'User'
);
GO
