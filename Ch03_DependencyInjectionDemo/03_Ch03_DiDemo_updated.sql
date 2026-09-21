/* ============================================================================
   PRN222 - Chuong 03: Dependency Injection in .NET
   Database: PRN222_Ch03_DiDemo

   Phien ban cap nhat cho bai tap:
   - CRUD Categories
   - CRUD Products
   - Tao Order
   - Thong ke tong doanh thu
   - Thong ke so don hang theo khoang thoi gian

   LUU Y:
   - Script nay dung de TAO LAI database schema tu dau.
   - Neu cac bang da ton tai, Orders -> Products -> Categories se bi xoa theo thu tu.
   - Khong dung ON DELETE CASCADE de tranh xoa nham Product/Order lich su.
   ============================================================================ */

IF DB_ID('PRN222_Ch03_DiDemo') IS NULL
BEGIN
    CREATE DATABASE PRN222_Ch03_DiDemo;
END
GO

USE PRN222_Ch03_DiDemo;
GO

/* --------------------------------------------------------------------------
   1. Xoa bang cu theo dung thu tu khoa ngoai
   -------------------------------------------------------------------------- */
IF OBJECT_ID('dbo.Orders', 'U') IS NOT NULL
    DROP TABLE dbo.Orders;
GO

IF OBJECT_ID('dbo.Products', 'U') IS NOT NULL
    DROP TABLE dbo.Products;
GO

IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL
    DROP TABLE dbo.Categories;
GO

/* --------------------------------------------------------------------------
   2. Categories
   -------------------------------------------------------------------------- */
CREATE TABLE dbo.Categories
(
    Id      INT IDENTITY(1,1) NOT NULL,
    Name    NVARCHAR(100)     NOT NULL,

    CONSTRAINT PK_Categories
        PRIMARY KEY CLUSTERED (Id),

    CONSTRAINT CK_Categories_Name_NotBlank
        CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

/* --------------------------------------------------------------------------
   3. Products
   -------------------------------------------------------------------------- */
CREATE TABLE dbo.Products
(
    Id          INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(150)     NOT NULL,
    Price       DECIMAL(18,2)     NOT NULL,
    Quantity    INT               NOT NULL,
    CategoryId  INT               NOT NULL,

    CONSTRAINT PK_Products
        PRIMARY KEY CLUSTERED (Id),

    CONSTRAINT FK_Products_Categories_CategoryId
        FOREIGN KEY (CategoryId)
        REFERENCES dbo.Categories (Id)
        ON DELETE NO ACTION,

    CONSTRAINT CK_Products_Name_NotBlank
        CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

    CONSTRAINT CK_Products_Price_NonNegative
        CHECK (Price >= 0),

    CONSTRAINT CK_Products_Quantity_NonNegative
        CHECK (Quantity >= 0)
);
GO

CREATE INDEX IX_Products_CategoryId
    ON dbo.Products (CategoryId);
GO

/* --------------------------------------------------------------------------
   4. Orders

   Moi dong Order trong bai hien tai dai dien cho viec ban mot Product.
   UnitPrice va TotalAmount duoc luu tai thoi diem ban de doanh thu lich su
   khong bi thay doi khi gia Product duoc sua sau nay.
   -------------------------------------------------------------------------- */
CREATE TABLE dbo.Orders
(
    Id           INT IDENTITY(1,1) NOT NULL,
    ProductId    INT               NOT NULL,
    Quantity     INT               NOT NULL,
    UnitPrice    DECIMAL(18,2)     NOT NULL,
    TotalAmount  DECIMAL(18,2)     NOT NULL,
    OrderDate    DATETIME2(0)      NOT NULL
        CONSTRAINT DF_Orders_OrderDate DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Orders
        PRIMARY KEY CLUSTERED (Id),

    CONSTRAINT FK_Orders_Products_ProductId
        FOREIGN KEY (ProductId)
        REFERENCES dbo.Products (Id)
        ON DELETE NO ACTION,

    CONSTRAINT CK_Orders_Quantity_Positive
        CHECK (Quantity > 0),

    CONSTRAINT CK_Orders_UnitPrice_NonNegative
        CHECK (UnitPrice >= 0),

    CONSTRAINT CK_Orders_TotalAmount_NonNegative
        CHECK (TotalAmount >= 0)
);
GO

CREATE INDEX IX_Orders_ProductId
    ON dbo.Orders (ProductId);
GO

CREATE INDEX IX_Orders_OrderDate
    ON dbo.Orders (OrderDate);
GO

/* --------------------------------------------------------------------------
   5. Du lieu goc - trung voi project demo ban dau
   -------------------------------------------------------------------------- */
SET IDENTITY_INSERT dbo.Categories ON;

INSERT INTO dbo.Categories (Id, Name)
VALUES
    (1, N'Laptop'),
    (2, N'Smartphone'),
    (3, N'Accessory');

SET IDENTITY_INSERT dbo.Categories OFF;
GO

SET IDENTITY_INSERT dbo.Products ON;

INSERT INTO dbo.Products (Id, Name, Price, Quantity, CategoryId)
VALUES
    (1, N'Dell Latitude 5450',     24990000.00, 12, 1),
    (2, N'MacBook Air M3',         31490000.00,  7, 1),
    (3, N'Samsung Galaxy S24',     19990000.00, 20, 2),
    (4, N'Logitech MX Master 3S',   2390000.00, 35, 3);

SET IDENTITY_INSERT dbo.Products OFF;
GO

/* --------------------------------------------------------------------------
   6. Kiem tra schema + du lieu
   -------------------------------------------------------------------------- */
PRINT N'===== CATEGORIES =====';
SELECT Id, Name
FROM dbo.Categories
ORDER BY Id;
GO

PRINT N'===== PRODUCTS =====';
SELECT
    p.Id,
    p.Name,
    p.Price,
    p.Quantity,
    p.CategoryId,
    c.Name AS Category
FROM dbo.Products AS p
INNER JOIN dbo.Categories AS c
    ON c.Id = p.CategoryId
ORDER BY p.Id;
GO

PRINT N'===== ORDERS =====';
SELECT
    o.Id,
    o.ProductId,
    p.Name AS Product,
    o.Quantity,
    o.UnitPrice,
    o.TotalAmount,
    o.OrderDate
FROM dbo.Orders AS o
INNER JOIN dbo.Products AS p
    ON p.Id = o.ProductId
ORDER BY o.Id;
GO

/* --------------------------------------------------------------------------
   7. Cac cau truy van thong ke dung de doi chieu khi test
   -------------------------------------------------------------------------- */
PRINT N'===== TOTAL REVENUE =====';
SELECT
    COALESCE(SUM(TotalAmount), 0) AS TotalRevenue
FROM dbo.Orders;
GO

/* Vi du thong ke so don theo khoang ngay:

DECLARE @FromDate DATE = '2026-09-01';
DECLARE @ToDate   DATE = '2026-09-30';

SELECT COUNT(*) AS OrderCount
FROM dbo.Orders
WHERE OrderDate >= @FromDate
  AND OrderDate < DATEADD(DAY, 1, @ToDate);

*/

PRINT N'Hoan tat: da tao Categories, Products, Orders va nap du lieu goc.';
GO
