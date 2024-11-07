CREATE OR ALTER PROCEDURE [NTangle].[spContactBatchTracking]
  @BatchTrackingId BIGINT,
  @ContactMinLsn BINARY(10),
  @ContactMaxLsn BINARY(10),
  @AddressMinLsn BINARY(10),
  @AddressMaxLsn BINARY(10),
  @VersionTrackingList AS NVARCHAR(MAX),
  @IdentifierList AS NVARCHAR(MAX),
  @HasDataLoss BIT
--  @IsComplete BIT
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Create or updates the batch tracking accordingly.
    IF @BatchTrackingId = 0
    BEGIN
      DECLARE @InsertedBatchTrackingId TABLE([BatchTrackingId] INT)

      INSERT INTO [NTangle].[ContactBatchTracking] (
          [ContactMinLsn],
          [ContactMaxLsn],
          [AddressMinLsn],
          [AddressMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        )
        OUTPUT inserted.BatchTrackingId INTO @InsertedBatchTrackingId
        VALUES (
          @ContactMinLsn,
          @ContactMaxLsn,
          @AddressMinLsn,
          @AddressMaxLsn,
          GETUTCDATE(),
          0,
          LOWER(CONVERT(NVARCHAR(64), NEWID())),
          @hasDataLoss
        )

        SELECT @BatchTrackingId = [BatchTrackingId] FROM @InsertedBatchTrackingId
    END
    ELSE
    BEGIN
      IF (@hasDataLoss = 1)
      BEGIN
        UPDATE [NTangle].[ContactBatchTracking]
          SET [HasDataLoss] = @hasDataLoss
          WHERE [BatchTrackingId] = @BatchTrackingId
      END
    END

    -- Return the *latest* batch tracking data.
    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [NTangle].[ContactBatchTracking] AS [_batch]
      WHERE [_batch].BatchTrackingId = @BatchTrackingId 

    -- Get the existing version tracking hashes.
    SELECT
        [_vt].[Key],
        [_vt].[Hash]
      FROM [NTangle].[VersionTracking] as [_vt]
      WHERE [_vt].[Schema] = N'Legacy' AND [_vt].[Table] = N'Contact' AND [_vt].[Key] IN (SELECT VALUE FROM OPENJSON(@VersionTrackingList))

    -- Get the existing global identifiers.
    SELECT * INTO #identifierList FROM OPENJSON(@IdentifierList) WITH ([Schema] NVARCHAR(50) '$.schema', [Table] NVARCHAR(127) '$.table', [Key] NVARCHAR(255) '$.key')

    SELECT [_i].[Schema], [_i].[Table], [_i].[Key], [n].[GlobalId]
      FROM [NTangle].[IdentifierMapping] AS [n]
      RIGHT OUTER JOIN #identifierList AS [_i] ON [n].[Schema] = [_i].[Schema] AND [n].[Table] = [_i].[Table] AND [n].[Key] = [_i].[Key]

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