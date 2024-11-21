BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Declare variables.
    DECLARE @hasDataLoss BIT = 0
    DECLARE @CustomerBaseMinLsn BINARY(10)

    -- Determine the minimum lsn.
    SET @CustomerBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Cust');

    -- Determine the 'minimum' depending on whether reprocessing or next and set/increment accordingly.
    IF @CustomerMinLsn IS NULL
    BEGIN
      SET @CustomerMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Cust');
    END
    ELSE
    BEGIN
      IF @BatchTrackingId = 0
      BEGIN
        SET @CustomerMinLsn = sys.fn_cdc_increment_lsn(@CustomerMaxLsn);
      END
    END

    -- Determine the maximum lsn.
    IF @BatchTrackingId = 0
    BEGIN
      SET @CustomerMaxLsn = sys.fn_cdc_get_max_lsn();
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    IF (@CustomerMinLsn < @CustomerBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CustomerMinLsn = @CustomerBaseMinLsn END ELSE BEGIN ;THROW 56010, 'Unexpected data loss error for ''Legacy.Cust''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: '[Legacy].[Cust]' - this determines overall operation type: 'create', 'update' or 'delete'.
    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [CustId] INT)
    DECLARE @hasChanges BIT = 0

    IF (@CustomerMinLsn <= @CustomerMaxLsn)
    BEGIN
      INSERT INTO #_changes
        SELECT TOP (@MaxQuerySize)
            [_cdc].[__$start_lsn] AS [_Lsn],
            [_cdc].[__$operation] AS [_Op],
            [_cdc].[CustId] AS [CustId]
          FROM cdc.fn_cdc_get_all_changes_Legacy_Cust(@CustomerMinLsn, @CustomerMaxLsn, 'all') AS [_cdc]
          WHERE ([_cdc].[is-private] IS NULL OR [_cdc].[is-private] = 0)
          ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CustomerMinLsn = MIN([_Lsn]), @CustomerMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Select the LSN ranges and whether there is data loss.
    SELECT
        @CustomerMinLsn AS [CustomerMinLsn],
        @CustomerMaxLsn AS [CustomerMaxLsn],
        @hasDataLoss AS [HasDataLoss],
        @hasChanges AS [HasChanges]

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      RETURN
    END

    -- Root table: '[Legacy].[Cust]' - uses LEFT OUTER JOIN's to get the deleted records.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_chg].[CustId] AS [Id],
        [c].[CustId] AS [TableKey_Id],
        [c].[Name] AS [Name],
        [c].[Email] AS [Email],
        [c].[is-deleted] AS [IsDeleted],
        [c].[RowVersion] AS [RowVersion],
        CASE WHEN EXISTS (SELECT 1 FROM [Legacy].[Cust] AS [__c] WHERE ([__c].[CustId] = [_chg].[CustId])) THEN CAST (0 AS BIT) ELSE CAST (1 AS BIT) END AS [_IsPhysicallyDeleted]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Cust] AS [c] ON ([c].[CustId] = [_chg].[CustId])
      ORDER BY [_Lsn] ASC

    RETURN
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END