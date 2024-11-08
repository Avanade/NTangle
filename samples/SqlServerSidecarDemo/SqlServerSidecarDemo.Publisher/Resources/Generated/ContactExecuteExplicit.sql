BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Declare variables.
    DECLARE @lsn BINARY(10)

    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [ContactId] INT)

    -- Simulate changes on the root table: '[Legacy].[Contact]'.
    IF @ContactKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #ContactKeysList FROM OPENJSON(@ContactKeysList) WITH ([ContactId] INT '$.CID')

      INSERT INTO #_changes
        SELECT
            @lsn AS [_Lsn],
            4 AS [_Op],
            [_cdc].[ContactId] AS [ContactId]
          FROM #ContactKeysList AS [_cdc]
    END

    -- Simulate changes on the related table: '[Legacy].[Address]'.
    IF @AddressKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #AddressKeysList FROM OPENJSON(@AddressKeysList) WITH ([AddressId] INT '$.AID')

      SELECT
          @lsn AS [_Lsn],
          4 AS [_Op],
          [c].[ContactId] AS [ContactId]
        INTO #a
        FROM #AddressKeysList AS [_cdc]
        INNER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([_cdc].[AddressId] = [c].[AddressId])

      IF (@@ROWCOUNT <> 0)
      BEGIN
        INSERT INTO #_changes
          SELECT * 
            FROM #a AS [_a]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[ContactId] = [_a].[ContactId])
      END
    END

    -- Root table: '[Legacy].[Contact]' - uses LEFT OUTER JOIN's to get the deleted records.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_chg].[ContactId] AS [CID],
        [c].[ContactId] AS [TableKey_CID],
        [c].[Name] AS [Name],
        [c].[Phone] AS [Phone],
        [c].[Email] AS [Email],
        [c].[Active] AS [Active],
        [c].[DontCallList] AS [DontCallList],
        [c].[AddressId] AS [AddressId],
        [c].[AlternateContactId] AS [AlternateContactId],
        [cm].[UniqueId] AS [UniqueId],
        CASE WHEN EXISTS (SELECT 1 FROM [Legacy].[Contact] AS [__c] WHERE ([__c].[ContactId] = [_chg].[ContactId])) THEN CAST (0 AS BIT) ELSE CAST (1 AS BIT) END AS [_IsPhysicallyDeleted]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      LEFT OUTER JOIN [Legacy].[ContactMapping] AS [cm] ON ([cm].[ContactId] = [c].[ContactId])

    -- Related table: '[Legacy].[Address]' - unique name 'Address' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [a].[AddressId] AS [AID],
        [a].[Street1] AS [Street1],
        [a].[Street2] AS [Street2],
        [a].[AlternateAddressId] AS [AlternateAddressId]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[Address] AS [a] ON ([a].[AddressId] = [c].[AddressId])

    RETURN
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END