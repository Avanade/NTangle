BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Declare variables.
    DECLARE @lsn BINARY(10)

    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [CustId] INT)

    -- Simulate changes on the root table: '[Legacy].[Cust]'.
    IF @CustomerKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #CustomerKeysList FROM OPENJSON(@CustomerKeysList) WITH ([CustId] INT '$.Id')

      INSERT INTO #_changes
        SELECT
            @lsn AS [_Lsn],
            4 AS [_Op],
            [_cdc].[CustId] AS [CustId]
          FROM #CustomerKeysList AS [_cdc]
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

    RETURN
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END