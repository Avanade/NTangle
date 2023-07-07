-- Enable CDC for database

IF (SELECT TOP 1 is_cdc_enabled FROM sys.databases WHERE [name] = DB_NAME()) = 0
BEGIN
    DECLARE @user NVARCHAR(256) = SUSER_NAME()
    EXEC sp_changedbowner 'sa'
    EXEC sys.sp_cdc_enable_db
    EXEC sp_changedbowner @user
END

GO

-- old schema

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE [name] = N'old')
BEGIN
    EXEC('CREATE SCHEMA [old]')
END

GO

-- contact_address/contact

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'old.contact_address'))
BEGIN
    CREATE TABLE [old].[contact_address] (
      [contact_address_id] INT NOT NULL PRIMARY KEY,
      [address_street_1] NVARCHAR(100) NULL,
      [address_street_2] NVARCHAR(100) NULL
    )
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'old.contact'))
BEGIN
    CREATE TABLE [old].[contact] (
      [contact_id] INT NOT NULL,
      [contact_name] NVARCHAR (200) NULL,
      [contact_phone] VARCHAR (15) NULL,
      [contact_email] VARCHAR (200) NULL,
      [contact_active] BIT NULL,
      [contact_no_calling] BIT NULL,
      [contact_addressid] INT NULL
      CONSTRAINT [pk_contact] PRIMARY KEY CLUSTERED ([contact_id] ASC),
      CONSTRAINT [fk_contact_contact_address] FOREIGN KEY ([contact_addressid]) REFERENCES [old].[contact_address] ([contact_address_id])
    );
END

GO