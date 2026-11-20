CREATE DATABASE RefeitorioDB;
GO

USE RefeitorioDB;
GO

-- Tabela Users (modificada conforme solicitado)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserNumber NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(200) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(20) NULL,
    ProfileImage NVARCHAR(200) NULL,
    Balance DECIMAL(10,2) NOT NULL DEFAULT 0,
    UserType NVARCHAR(20) NOT NULL CHECK (UserType IN ('Admin', 'Staff', 'Student'))
);

-- Tabela Categories
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- Tabela Products
CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Price DECIMAL(10,2) NOT NULL,
    ImageUrl NVARCHAR(200) NULL,
    CategoryId INT NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    IsVegetarian BIT NOT NULL DEFAULT 0,
    Stock INT NOT NULL DEFAULT 0,
    Calories INT NULL,
    Protein DECIMAL(5,2) NULL,
    Carbs DECIMAL(5,2) NULL,
    Fat DECIMAL(5,2) NULL,
    Allergens NVARCHAR(200) NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Tabela WeeklyMenu (removido DayOfWeek)
CREATE TABLE WeeklyMenu (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MenuDate DATE NOT NULL,
    IsVegetarian BIT NOT NULL,
    MainDishId INT NOT NULL,
    SoupId INT NULL,
    DessertId INT NULL,
    Price DECIMAL(10,2) NOT NULL,
    MaxReservations INT NOT NULL DEFAULT 150,
    CONSTRAINT FK_WeeklyMenu_MainDish FOREIGN KEY (MainDishId) REFERENCES Products(Id),
    CONSTRAINT FK_WeeklyMenu_Soup FOREIGN KEY (SoupId) REFERENCES Products(Id),
    CONSTRAINT FK_WeeklyMenu_Dessert FOREIGN KEY (DessertId) REFERENCES Products(Id)
);

-- Tabela Tickets
CREATE TABLE Tickets (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    MenuId INT NOT NULL,
    ReservationDate DATETIME NOT NULL DEFAULT GETDATE(),
    IsUsed BIT NOT NULL DEFAULT 0,
    UsedAt DATETIME NULL,
    ValidatedByStaffId INT NULL,
    CONSTRAINT FK_Tickets_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Tickets_WeeklyMenu FOREIGN KEY (MenuId) REFERENCES WeeklyMenu(Id),
    CONSTRAINT FK_Tickets_Staff FOREIGN KEY (ValidatedByStaffId) REFERENCES Users(Id)
);

-- Tabela Cart
CREATE TABLE Cart (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Cart_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Cart_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Tabela Orders
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) NOT NULL CHECK (Status IN ('Pending', 'Ready', 'Delivered', 'Canceled')),
    DeliveredAt DATETIME NULL,
    DeliveredByStaffId INT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT FK_Orders_Staff FOREIGN KEY (DeliveredByStaffId) REFERENCES Users(Id)
);

-- Tabela OrderItems
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- Tabela Favorites
CREATE TABLE Favorites (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    AddedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Favorites_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Favorites_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT UQ_UserProduct UNIQUE (UserId, ProductId)
);

-- Índices para performance
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_WeeklyMenu_MenuDate ON WeeklyMenu(MenuDate);
CREATE INDEX IX_Tickets_UserId ON Tickets(UserId);
CREATE INDEX IX_Tickets_MenuId ON Tickets(MenuId);
CREATE INDEX IX_Cart_UserId ON Cart(UserId);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_Favorites_UserId ON Favorites(UserId);

-- Seed data
-- 1. Inserir Categories (usar dados originais)
INSERT INTO Categories (Name) VALUES 
('Bebidas'),
('Snacks'),
('Refeições'),
('Sobremesas');

-- 2. Inserir Users (dados atualizados com os novos campos)
-- Demo accounts (local demo only). Password for all three: Demo-2026!
-- Passwords are stored as ASP.NET Core Identity PasswordHasher hashes, never in plain text.
INSERT INTO Users (UserNumber, Password, Name, Email, Phone, ProfileImage, Balance, UserType) VALUES 
('admin', 'AQAAAAEAAYagAAAAEF/uCGaabR2sdvkoY8IINWIgS9h7BCBMAcIZHPWc+YOxM3dn2MeJPL6VE2IpoGVvbA==', 'Demo Admin', 'admin@example.com', '', '', 50.00, 'Admin'),
('student', 'AQAAAAEAAYagAAAAEOlC9L26Sqaw25dHSNC3FI5gY7wNcKwPqIdQdJDMhXqS/n6ZRdHwkEiK+V/GWPz2Yg==', 'Demo Student', 'student@example.com', '', '', 75.50, 'Student'),
('func', 'AQAAAAEAAYagAAAAEOAuXkK0h0QzIFv1GHMGlvVdnKxOkIZh1BsjH+Zc1nnf0CzLvw6eBAZF3srT/hJVQw==', 'Demo Staff', 'staff@example.com', '', '', 100.00, 'Staff');

-- 3. Inserir Products (apenas os 4 produtos originais)
INSERT INTO Products (Name, Description, Price, CategoryId, ImageUrl, IsAvailable, IsVegetarian, Stock, Calories, Protein, Carbs, Fat, Allergens) VALUES 
('Coca-Cola', 'Refrigerante 330ml', 1.20, 1, '/images/coca_cola.png', 1, 1, 100, 139, 0, 35, 0, ''),
('Sandwich de Frango', 'Sandwich com frango grelhado', 3.50, 2, '/images/sanduiche.png', 1, 0, 50, 280, 25, 30, 12, 'Glúten'),
('Salada Mista', 'Salada fresca com vegetais', 2.80, 3, '/images/salada.png', 1, 1, 30, 120, 5, 15, 8, ''),
('Chocolate', 'Barra de chocolate 50g', 1.50, 4, '/images/chocolate.png', 1, 1, 75, 230, 3, 25, 15, 'Lactose');

-- 4. Inserir WeeklyMenu (pelo menos 5 registros)
INSERT INTO WeeklyMenu (MenuDate, IsVegetarian, MainDishId, SoupId, DessertId, Price, MaxReservations) VALUES
('2026-01-20', 0, 2, 3, 4, 8.50, 150),  -- Sandwich, Salada, Chocolate
('2026-01-20', 1, 3, 3, 4, 7.80, 150),  -- Salada, Salada, Chocolate
('2026-01-21', 0, 2, 3, 4, 8.80, 150),  -- Sandwich, Salada, Chocolate
('2026-01-21', 1, 3, 3, 1, 8.00, 150),  -- Salada, Salada, Coca-Cola
('2026-01-22', 0, 2, 3, 4, 8.50, 150),  -- Sandwich, Salada, Chocolate
('2026-01-22', 1, 3, 3, 1, 7.80, 150),  -- Salada, Salada, Coca-Cola
('2026-01-23', 0, 2, 3, 4, 8.80, 150),  -- Sandwich, Salada, Chocolate
('2026-01-23', 1, 3, 3, 1, 8.00, 150);  -- Salada, Salada, Coca-Cola

-- 5. Inserir Tickets (pelo menos 5 registros)
INSERT INTO Tickets (UserId, MenuId, ReservationDate, IsUsed, UsedAt, ValidatedByStaffId) VALUES
(2, 1, '2026-01-19 08:30:00', 0, NULL, NULL),
(2, 3, '2026-01-19 09:15:00', 0, NULL, NULL),
(2, 5, '2026-01-20 08:45:00', 0, NULL, NULL),
(2, 7, '2026-01-21 08:20:00', 0, NULL, NULL),
(2, 2, '2026-01-19 10:00:00', 0, NULL, NULL);

-- 6. Inserir Cart (pelo menos 5 registros)
INSERT INTO Cart (UserId, ProductId, Quantity, CreatedAt) VALUES
(2, 1, 2, '2026-01-15 11:30:00'),
(2, 2, 1, '2026-01-15 11:30:00'),
(2, 4, 3, '2026-01-15 11:30:00'),
(3, 1, 1, '2026-01-15 12:15:00'),
(3, 3, 2, '2026-01-15 12:15:00');

-- 7. Inserir Orders (pelo menos 5 registros)
INSERT INTO Orders (UserId, OrderDate, TotalAmount, Status, DeliveredAt, DeliveredByStaffId) VALUES
(2, '2026-01-14 12:30:00', 7.20, 'Delivered', '2026-01-14 12:45:00', 3),
(2, '2026-01-15 10:45:00', 5.00, 'Delivered', '2026-01-15 11:00:00', 3),
(2, '2026-01-16 13:20:00', 6.50, 'Ready', NULL, NULL),
(2, '2026-01-17 09:30:00', 10.30, 'Pending', NULL, NULL),
(3, '2026-01-17 11:45:00', 4.50, 'Pending', NULL, NULL);

-- 8. Inserir OrderItems (pelo menos 5 registros)
INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES
(1, 1, 2, 1.20),  -- 2 Coca-Colas
(1, 4, 3, 1.50),  -- 3 Chocolates
(2, 2, 1, 3.50),  -- 1 Sandwich
(2, 1, 1, 1.20),  -- 1 Coca-Cola
(3, 3, 1, 2.80),  -- 1 Salada
(3, 4, 2, 1.50),  -- 2 Chocolates
(4, 2, 1, 3.50),  -- 1 Sandwich
(4, 1, 4, 1.20),  -- 4 Coca-Colas
(5, 2, 1, 3.50),  -- 1 Sandwich
(5, 3, 1, 2.80);  -- 1 Salada

-- 9. Inserir Favorites (pelo menos 5 registros)
INSERT INTO Favorites (UserId, ProductId, AddedAt) VALUES
(2, 1, '2026-01-10 09:30:00'),  -- demo student favorita Coca-Cola
(2, 2, '2026-01-10 09:30:00'),  -- demo student favorita Sandwich
(2, 4, '2026-01-10 09:30:00'),  -- demo student favorita Chocolate
(2, 3, '2026-01-12 14:15:00'),  -- demo student favorita Salada
(3, 1, '2026-01-12 14:15:00');  -- demo staff favorita Coca-Cola

-- Selects para cada tabela
SELECT * FROM Users;
SELECT * FROM Categories;
SELECT * FROM Products;
SELECT * FROM WeeklyMenu;
SELECT * FROM Tickets;
SELECT * FROM Cart;
SELECT * FROM Orders;
SELECT * FROM OrderItems;
SELECT * FROM Favorites;