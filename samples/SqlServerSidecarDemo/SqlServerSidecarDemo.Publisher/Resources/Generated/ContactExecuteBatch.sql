BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Declare variables.
    DECLARE @hasDataLoss BIT = 0
    DECLARE @ContactBaseMinLsn BINARY(10)
    DECLARE @AddressBaseMinLsn BINARY(10)

    -- Determine the minimum lsn.
    SET @ContactBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Contact');
    SET @AddressBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Address');

    -- Determine the 'minimum' depending on whether reprocessing or next and set/increment accordingly.
    IF @ContactMinLsn IS NULL
    BEGIN
      SET @ContactMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Contact');
      SET @AddressMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Address');
    END
    ELSE
    BEGIN
      IF @BatchTrackingId = 0
      BEGIN
        SET @ContactMinLsn = sys.fn_cdc_increment_lsn(@ContactMaxLsn);
        SET @AddressMinLsn = sys.fn_cdc_increment_lsn(@AddressMaxLsn);
      END
    END

    -- Determine the maximum lsn.
    IF @BatchTrackingId = 0
    BEGIN
      SET @ContactMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @AddressMaxLsn = @ContactMaxLsn
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    IF (@ContactMinLsn < @ContactBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @ContactMinLsn = @ContactBaseMinLsn END ELSE BEGIN ;THROW 56010, 'Unexpected data loss error for ''Legacy.Contact''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@AddressMinLsn < @AddressBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @AddressMinLsn = @AddressBaseMinLsn END ELSE BEGIN ;THROW 56010, 'Unexpected data loss error for ''Legacy.Address''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: '[Legacy].[Contact]' - this determines overall operation type: 'create', 'update' or 'delete'.
    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [ContactId] INT)
    DECLARE @hasChanges BIT = 0

    IF (@ContactMinLsn <= @ContactMaxLsn)
    BEGIN
      INSERT INTO #_changes
        SELECT TOP (@MaxQuerySize)
            [_cdc].[__$start_lsn] AS [_Lsn],
            [_cdc].[__$operation] AS [_Op],
            [_cdc].[ContactId] AS [ContactId]
          FROM cdc.fn_cdc_get_all_changes_Legacy_Contact(@ContactMinLsn, @ContactMaxLsn, 'all') AS [_cdc]
          ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @ContactMinLsn = MIN([_Lsn]), @ContactMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: '[Legacy].[Address]' - unique name 'Address' - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@AddressMinLsn <= @AddressMaxLsn)
    BEGIN
      DECLARE @aMaxQuerySize INT = CEILING(@MaxQuerySize * 1.5)
      SELECT TOP (@aMaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [c].[ContactId] AS [ContactId]
        INTO #a
        FROM cdc.fn_cdc_get_all_changes_Legacy_Address(@AddressMinLsn, @AddressMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Contact] AS [c] WITH (NOLOCK) ON ([_cdc].[AddressId] = [c].[AddressId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @AddressMinLsn = MIN([_Lsn]), @AddressMaxLsn = MAX([_Lsn]) FROM #a
    
        INSERT INTO #_changes
          SELECT *
            FROM #a AS [_a]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[ContactId] = [_a].[ContactId])
      END
    END

    -- Select the LSN ranges and whether there is data loss.
    SELECT
        @ContactMinLsn AS [ContactMinLsn],
        @ContactMaxLsn AS [ContactMaxLsn],
        @AddressMinLsn AS [AddressMinLsn],
        @AddressMaxLsn AS [AddressMaxLsn],
        @hasDataLoss AS [HasDataLoss],
        @hasChanges AS [HasChanges]

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      RETURN
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
      ORDER BY [_Lsn] ASC

    -- Related table: '[Legacy].[Address]' - unique name 'Address' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [a].[AddressId] AS [AID],
        [a].[Street1] AS [Street1],
        [a].[Street2] AS [Street2],
        [a].[AlternateAddressId] AS [AlternateAddressId]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Contact] AS [c] ON ([c].[ContactId] = [_chg].[ContactId])
      INNER JOIN [Legacy].[Address] AS [a] ON ([a].[AddressId] = [c].[AddressId])
      WHERE [_chg].[_Op] <> 1

    RETURN
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END