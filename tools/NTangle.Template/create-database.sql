-- Create the database.
CREATE DATABASE FooBar

GO

USE FooBar

GO

-- Enable CDC for database

DECLARE @user NVARCHAR(256) = SUSER_NAME()
EXEC sp_changedbowner 'sa'
EXEC sys.sp_cdc_enable_db
EXEC sp_changedbowner @user

GO

-- Create the schema.

CREATE SCHEMA [Legacy]

GO

-- Contact/AddressType/Address

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Contact'))
BEGIN
    CREATE TABLE [Legacy].[Contact] (
      [ContactId] INT NOT NULL PRIMARY KEY,
      [Name] NVARCHAR (200) NULL,
      [Phone] VARCHAR (15) NULL,
      [Email] VARCHAR (200) NULL
    )
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.AddressType'))
BEGIN
    CREATE TABLE [Legacy].[AddressType] (
      [AddressTypeId] INT NOT NULL PRIMARY KEY,
      [Code] NVARCHAR(20) NOT NULL UNIQUE,
      [Text] NVARCHAR(100)
    )
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Address'))
BEGIN
    CREATE TABLE [Legacy].[Address] (
      [AddressId] INT NOT NULL PRIMARY KEY,
      [ContactId] INT NOT NULL,
      [AddressTypeId] INT NOT NULL,
      [Street1] NVARCHAR(100),
      [Street2] NVARCHAR(100),
      CONSTRAINT [FK_Legacy_Address_Contact] FOREIGN KEY ([ContactId]) REFERENCES [Legacy].[Contact] ([ContactId]),
      CONSTRAINT [FK_Legacy_Address_AddressType] FOREIGN KEY ([AddressTypeId]) REFERENCES [Legacy].[AddressType] ([AddressTypeId]),
    )
END

GO

-- Create some initial data.

INSERT INTO [Legacy].[AddressType] ([AddressTypeId], [Code], [Text]) VALUES (88, 'H', 'Home')
INSERT INTO [Legacy].[AddressType] ([AddressTypeId], [Code], [Text]) VALUES (89, 'P', 'Postal')
INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone]) VALUES (1, 'Bob', '123')
INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone]) VALUES (2, 'Jane', '456')
INSERT INTO [Legacy].[Contact] ([ContactId], [Name], [Phone]) VALUES (3, 'Sarah', '789')
INSERT INTO [Legacy].[Address] ([AddressId], [ContactId], [AddressTypeId], [Street1], [Street2]) VALUES (11, 1, 88, '1st', 'Seattle')
INSERT INTO [Legacy].[Address] ([AddressId], [ContactId], [AddressTypeId], [Street1], [Street2]) VALUES (12, 1, 89, 'Main', 'Redmond')
INSERT INTO [Legacy].[Address] ([AddressId], [ContactId], [AddressTypeId], [Street1], [Street2]) VALUES (21, 2, 88, 'Simpsons', 'Brisbane')