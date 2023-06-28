-- Create table: [New].[Contact]

BEGIN TRANSACTION

CREATE TABLE [New].[Contact] (
  [ContactId] INT NOT NULL PRIMARY KEY,
  [Name] NVARCHAR(200) NULL,
  [Phone] NVARCHAR(50) NULL,
  [Email] NVARCHAR(200) NULL,
  [IsActive] BIT NULL,
  [NoCallList] BIT NULL,
  [AddressStreet1] NVARCHAR(100) NULL,
  [AddressStreet2] NVARCHAR(100) NULL,
  [RowVersion] TIMESTAMP NOT NULL
);

COMMIT TRANSACTION