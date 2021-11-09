/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- Enable CDC for database

IF (SELECT TOP 1 is_cdc_enabled FROM sys.databases WHERE [name] = N'$(DatabaseName)') = 0
BEGIN
  EXEC sp_changedbowner 'sa'
  EXEC sys.sp_cdc_enable_db
END

-- Legacy schema

GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'Legacy')
BEGIN
    EXEC('CREATE SCHEMA [Legacy]')
END

GO

-- Posts/Comments/Tags

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Posts'))
BEGIN
    CREATE TABLE [Legacy].[Posts] (
      [PostsId] INT NOT NULL PRIMARY KEY,
      [Text] NVARCHAR(256) NULL UNIQUE,
      [Date] DATE NULL
    );
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Comments'))
BEGIN
    CREATE TABLE [Legacy].[Comments] (
      [CommentsId] INT NOT NULL PRIMARY KEY,
      [PostsId] INT NOT NULL,
      [Text] NVARCHAR(256) NULL UNIQUE,
      [Date] DATE NULL
    );
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Tags'))
BEGIN
    CREATE TABLE [Legacy].[Tags] (
      [TagsId] INT NOT NULL PRIMARY KEY,
      [ParentType] NVARCHAR(1) NOT NULL,
      [ParentId] INT NOT NULL,
      [Text] NVARCHAR(50) NOT NULL
    );
END

GO

-- Address/Contact/ContactMapping

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Address'))
BEGIN
    CREATE TABLE [Legacy].[Address] (
      [AddressId] INT NOT NULL PRIMARY KEY,
      Street1 NVARCHAR(100),
      Street2 NVARCHAR(100),
      AlternateAddressId INT NULL
    )
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Contact'))
BEGIN
    CREATE TABLE [Legacy].[Contact] (
      [ContactId] INT NOT NULL,
      [Name] NVARCHAR (200) NULL,
      [Phone] VARCHAR (15) NULL,
      [Email] VARCHAR (200) NULL,
      [Active] BIT NULL,
      [DontCallList] BIT NULL,
      [AddressId] INT NULL,
      CONSTRAINT [PK_Contact] PRIMARY KEY CLUSTERED ([ContactId] ASC),
      CONSTRAINT [FK_Contact_Address] FOREIGN KEY ([AddressId]) REFERENCES [Legacy].[Address] ([AddressId])
    );
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.ContactMapping'))
BEGIN
    CREATE TABLE [Legacy].[ContactMapping] (
      [ContactMappingId] INT PRIMARY KEY NONCLUSTERED ([ContactMappingId] ASC),
      [ContactId] INT NOT NULL,
      [UniqueId] UNIQUEIDENTIFIER NOT NULL,
      CONSTRAINT [IX_Legacy_ContactMapping_ContactId] UNIQUE CLUSTERED ([ContactId]),
      CONSTRAINT [IX_Legacy_ContactMapping_UniqueId] UNIQUE ([UniqueId])
    );
END