USE BidRushDb;
GO

INSERT INTO dbo.Categories (Name)
SELECT 'Electronics'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Electronics'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Vehicles'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Vehicles'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Fashion'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Fashion'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Home & Garden'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Home & Garden'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Collectibles'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Collectibles'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Sports & Recreation'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Sports & Recreation'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Books'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Books'
);

INSERT INTO dbo.Categories (Name)
SELECT 'Gaming'
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categories
    WHERE Name = 'Gaming'
);
GO