/*
  AFMarket local demo database

  WARNING: this script deletes and recreates the database named AFMarketDB.
  Run it only against a local/development SQL Server instance.
*/

USE master;
GO

IF DB_ID(N'AFMarketDB') IS NOT NULL
BEGIN
    ALTER DATABASE AFMarketDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AFMarketDB;
END
GO

CREATE DATABASE AFMarketDB;
GO

USE AFMarketDB;
GO

CREATE TABLE [User] (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    role VARCHAR(50) NOT NULL,
    username VARCHAR(50) NOT NULL UNIQUE,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    balance DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    registration_date DATETIME NOT NULL DEFAULT GETUTCDATE()
);
GO

CREATE TABLE Category (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(50) NOT NULL
);
GO

CREATE TABLE Product (
    product_id INT IDENTITY(1,1) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    stock INT NOT NULL DEFAULT 0 CHECK (stock >= 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    category_id INT,
    details NVARCHAR(MAX),
    CONSTRAINT FK_Product_Category
        FOREIGN KEY (category_id) REFERENCES Category(category_id)
);
GO

CREATE TABLE Favorite (
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    PRIMARY KEY (user_id, product_id),
    CONSTRAINT FK_Favorite_User
        FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT FK_Favorite_Product
        FOREIGN KEY (product_id) REFERENCES Product(product_id)
);
GO

CREATE TABLE CartItem (
    cart_item_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    CONSTRAINT FK_CartItem_User
        FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CONSTRAINT FK_CartItem_Product
        FOREIGN KEY (product_id) REFERENCES Product(product_id)
);
GO

CREATE TABLE [Transaction] (
    transaction_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    transaction_date DATETIME NOT NULL DEFAULT GETUTCDATE(),
    description VARCHAR(255),
    CONSTRAINT FK_Transaction_User
        FOREIGN KEY (user_id) REFERENCES [User](user_id)
);
GO

CREATE TABLE [Order] (
    order_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    order_date DATETIME NOT NULL DEFAULT GETUTCDATE(),
    total_amount DECIMAL(10,2) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    CONSTRAINT FK_Order_User
        FOREIGN KEY (user_id) REFERENCES [User](user_id)
);
GO

CREATE TABLE OrderItem (
    order_item_id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL CHECK (quantity > 0),
    price DECIMAL(10,2) NOT NULL CHECK (price >= 0),
    CONSTRAINT FK_OrderItem_Order
        FOREIGN KEY (order_id) REFERENCES [Order](order_id),
    CONSTRAINT FK_OrderItem_Product
        FOREIGN KEY (product_id) REFERENCES Product(product_id)
);
GO

CREATE TABLE Promotion (
    promotion_id INT IDENTITY(1,1) PRIMARY KEY,
    product_id INT NOT NULL,
    discount_percent DECIMAL(5,2) NOT NULL
        CHECK (discount_percent BETWEEN 0 AND 100),
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    CONSTRAINT CK_Promotion_Dates CHECK (end_date >= start_date),
    CONSTRAINT FK_Promotion_Product
        FOREIGN KEY (product_id) REFERENCES Product(product_id)
);
GO

INSERT INTO Category (name) VALUES
('Fruits'),
('Vegetables'),
('Dairy'),
('Bakery');
GO

INSERT INTO Product (name, stock, price, category_id, details) VALUES
('Apple', 100, 0.50, 1, 'Fresh apple'),
('Banana', 150, 0.30, 1, 'Fresh banana'),
('Carrot', 200, 0.20, 2, 'Fresh carrot'),
('Milk', 80, 1.20, 3, 'One litre of milk'),
('Bread', 50, 1.00, 4, 'Fresh bread'),
('Croissant', 40, 0.80, 4, 'Butter croissant');
GO

/*
  Register a user through the web interface. To test the admin-only product and
  user management screens, promote that registered account locally:

  UPDATE [User] SET role = 'Admin' WHERE username = 'your-username';
*/
