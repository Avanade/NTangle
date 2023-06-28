BEGIN TRANSACTION

UPDATE [New].[Contact] WITH (UPDLOCK, SERIALIZABLE)
  SET [Name] = @Name,
	  [Phone] = @Phone,
	  [Email] = @Email,
	  [IsActive] = @IsActive,
	  [NoCallList] = @NoCallList,
	  [AddressStreet1] = @AddressStreet1,
	  [AddressStreet2] = @AddressStreet2
  WHERE [ContactId] = @ContactId

IF @@ROWCOUNT = 0
BEGIN
  INSERT INTO [New].[Contact] (
	[ContactId],
	[Name],
	[Phone],
	[Email],
	[IsActive],
	[NoCallList],
	[AddressStreet1],
	[AddressStreet2]
  )
  VALUES (
	@ContactId,
	@Name,
	@Phone,
	@Email,
	@IsActive,
	@NoCallList,
	@AddressStreet1,
	@AddressStreet2
  )
END

COMMIT TRANSACTION