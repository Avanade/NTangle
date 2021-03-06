CREATE PROCEDURE [NTangle].[spIdentifierMappingCreate]
  @IdentifierList AS [NTangle].[udtIdentifierMappingList] READONLY
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */
 
  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Insert the Identifier only where a value does not already currently exist.
    INSERT INTO [NTangle].[IdentifierMapping]
        ([Schema], [Table], [Key], [GlobalId])
      SELECT [n].[Schema], [n].[Table], [n].[Key], [n].[GlobalId]
        FROM @IdentifierList AS [n]
        WHERE NOT EXISTS (SELECT 0 FROM [NTangle].[IdentifierMapping] AS [o]
                            WHERE [n].[Schema] = [o].[Schema] AND [n].[Table] = [o].[Table] AND [n].[Key] = [o].[Key])

    -- Get the latest (current) values as some may already have had a global idenfifier created (i.e. not inserted above).
    SELECT [n].[Schema], [n].[Table], [n].[Key], [n].[GlobalId]
      FROM [NTangle].[IdentifierMapping] AS [n]
      INNER JOIN @IdentifierList AS [o] ON [n].[Schema] = [o].[Schema] AND [n].[Table] = [o].[Table] AND [n].[Key] = [o].[Key]

    -- Commit the transaction.
    COMMIT TRANSACTION
    RETURN 0
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END