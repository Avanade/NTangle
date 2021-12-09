{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle }}
CREATE PROCEDURE [{{CdcSchema}}].[{{ExecuteStoredProcedure}}]
  @MaxQuerySize BIGINT = 100,         -- Maximum size of query to limit the number of changes to a manageable batch (performance vs failure trade-off).
  @ContinueWithDataLoss BIT = 0       -- Ignores data loss and continues; versus throwing an error.
AS
BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Wrap in a transaction.
    BEGIN TRANSACTION

    -- Declare variables.
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{#each CdcJoins}}
    DECLARE @{{pascal Name}}BaseMinLsn BINARY(10), @{{pascal Name}}MinLsn BINARY(10), @{{pascal Name}}MaxLsn BINARY(10)
{{/each}}
    DECLARE @BatchTrackingId INT

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @{{pascal Name}}MinLsn = [_batch].[{{pascal Name}}MinLsn],
        @{{pascal Name}}MaxLsn = [_batch].[{{pascal Name}}MaxLsn],
{{#each CdcJoins}}
        @{{pascal Name}}MinLsn = [_batch].[{{pascal Name}}MinLsn],
        @{{pascal Name}}MaxLsn = [_batch].[{{pascal Name}}MaxLsn],
{{/each}}
        @BatchTrackingId = [BatchTrackingId]
      FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
      WHERE [_batch].[IsComplete] = 0 
      ORDER BY [_batch].[BatchTrackingId]

    -- There should never be more than one incomplete batch.
    IF @@ROWCOUNT > 1
    BEGIN
      ;THROW 56002, 'There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.', 1
    END

    -- Get the latest 'base' minimum.
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Table}}');
{{#each CdcJoins}}
    SET @{{pascal Name}}BaseMinLsn = sys.fn_cdc_get_min_lsn('{{Schema}}_{{Table}}');
{{/each}}

    -- Where there is no incomplete batch then the next should be created/processed.
    IF (@BatchTrackingId IS NULL)
    BEGIN
      -- Get the last batch processed.
      SELECT TOP 1
          @{{pascal Name}}MinLsn = [_batch].[{{pascal Name}}MaxLsn]{{#ifne CdcJoins.Count 0}},{{/ifne}}
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn = [_batch].[{{pascal Name}}MaxLsn]{{#unless @last}},{{/unless}}
{{/each}}
        FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
        ORDER BY [_batch].[IsComplete] ASC, [_batch].[BatchTrackingId] DESC

      IF (@@ROWCOUNT = 0) -- No previous batch; i.e. is the first time!
      BEGIN
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{#each CdcJoins}}
        SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn;
{{/each}}
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{#each CdcJoins}}
        SET @{{pascal Name}}MinLsn = sys.fn_cdc_increment_lsn(@{{pascal Name}}MinLsn)
{{/each}}
      END

      -- Get the maximum LSN.
      SET @{{pascal Name}}MaxLsn = sys.fn_cdc_get_max_lsn();
{{#each CdcJoins}}
      SET @{{pascal Name}}MaxLsn = @{{pascal Parent.Name}}MaxLsn
{{/each}}

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT = 0

    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''{{Schema}}.{{Table}}''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
{{#each CdcJoins}}
    IF (@{{pascal Name}}MinLsn < @{{pascal Name}}BaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @{{pascal Name}}MinLsn = @{{pascal Name}}BaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''{{Schema}}.{{Table}}''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
{{/each}}

    -- Find changes on the root table: '[{{Schema}}].[{{Table}}]' - this determines overall operation type: 'create', 'update' or 'delete'.
    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, {{#each PrimaryKeyColumns}}[{{Name}}] {{upper DbColumn.Type}}{{#unless @last}}, {{/unless}}{{/each}})
    DECLARE @hasChanges BIT = 0

    IF (@{{pascal Name}}MinLsn <= @{{pascal Name}}MaxLsn)
    BEGIN
      INSERT INTO #_changes
        SELECT TOP (@MaxQuerySize)
            [_cdc].[__$start_lsn] AS [_Lsn],
            [_cdc].[__$operation] AS [_Op],
{{#each PrimaryKeyColumns}}
            [_cdc].[{{Name}}] AS [{{Name}}]{{#unless @last}},{{/unless}}
{{/each}}
          FROM cdc.fn_cdc_get_all_changes_{{Schema}}_{{Name}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
          ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

{{#each CdcJoins}}
    -- Find changes on related table: '[{{Schema}}].[{{Table}}]' - unique name '{{Name}}' - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@{{pascal Name}}MinLsn <= @{{pascal Name}}MaxLsn)
    BEGIN
{{#ifne QuerySizeMultiplier 1}}
      DECLARE @{{Alias}}MaxQuerySize INT = CEILING(@MaxQuerySize * {{QuerySizeMultiplier}})
{{/ifne}}
      SELECT TOP ({{#ifeq QuerySizeMultiplier 1}}@MaxQuerySize{{else}}@{{Alias}}MaxQuerySize{{/ifeq}})
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
  {{#each Parent.PrimaryKeyColumns}}
          [{{Parent.Alias}}].[{{Name}}] AS [{{Name}}]{{#unless @last}},{{/unless}}
  {{/each}}
        INTO #{{Alias}}
        FROM cdc.fn_cdc_get_all_changes_{{Schema}}_{{Table}}(@{{pascal Name}}MinLsn, @{{pascal Name}}MaxLsn, 'all') AS [_cdc]
  {{#each JoinHierarchy}}
        INNER JOIN [{{JoinToSchema}}].[{{JoinTo}}] AS [{{JoinToAlias}}] WITH (NOLOCK) ON ({{#each On}}{{#unless @first}} AND {{/unless}}{{#if Parent.IsFirstInJoinHierarchy}}[_cdc]{{else}}[{{Parent.Alias}}]{{/if}}.[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @{{pascal Name}}MinLsn = MIN([_Lsn]), @{{pascal Name}}MaxLsn = MAX([_Lsn]) FROM #{{Alias}}

        INSERT INTO #_changes
          SELECT * 
            FROM #{{Alias}} AS [_{{Alias}}]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE {{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[_chg].[{{Name}}] = [_{{../Alias}}].[{{Name}}]{{/each}})
      END
    END

{{/each}}
    -- Create a new batch where not processing an existing.
    IF (@BatchTrackingId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
    BEGIN
      DECLARE @InsertedBatchTrackingId TABLE([BatchTrackingId] INT)

      INSERT INTO [{{CdcSchema}}].[{{BatchTrackingTable}}] (
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{#each CdcJoins}}
          [{{pascal Name}}MinLsn],
          [{{pascal Name}}MaxLsn],
{{/each}}
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        ) 
        OUTPUT inserted.BatchTrackingId INTO @InsertedBatchTrackingId
        VALUES (
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{#each CdcJoins}}
          @{{pascal Name}}MinLsn,
          @{{pascal Name}}MaxLsn,
{{/each}}
          GETUTCDATE(),
          0,
          LOWER(CONVERT(NVARCHAR(64), NEWID())),
          @hasDataLoss
        )

        SELECT @BatchTrackingId = [BatchTrackingId] FROM @InsertedBatchTrackingId
    END
    ELSE
    BEGIN
      IF (@BatchTrackingId IS NOT NULL AND @hasDataLoss = 1)
      BEGIN
        UPDATE [{{CdcSchema}}].[{{BatchTrackingTable}}] 
          SET [HasDataLoss] = @hasDataLoss
          WHERE [BatchTrackingId] = @BatchTrackingId
      END
    END

    -- Return the *latest* batch tracking data.
    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [{{CdcSchema}}].[{{BatchTrackingTable}}] AS [_batch]
      WHERE [_batch].BatchTrackingId = @BatchTrackingId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: '[{{Schema}}].[{{Table}}]' - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous 'TrackingHash' value.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
{{#if IdentifierMapping}}
        [_im].[GlobalId] AS [GlobalId],
{{/if}}
{{#each PrimaryKeyColumns}}
        [_chg].[{{Name}}] AS [{{NameAlias}}],
{{/each}}
{{#each PrimaryKeyColumns}}
        [{{Parent.Alias}}].[{{Name}}] AS [TableKey_{{NameAlias}}]{{#if @last}}{{#ifne Parent.SelectedColumnsExcludingPrimaryKey.Count 0}},{{/ifne}}{{else}},{{/if}}
{{/each}}
{{#each SelectedColumnsExcludingPrimaryKey}}
        [{{#ifval IdentifierMappingAlias}}{{IdentifierMappingAlias}}{{else}}{{Parent.Alias}}{{/ifval}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
{{/each}}
{{#each JoinNonCdcChildren}}
  {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#unless @../last}},{{/unless}}{{/unless}}
  {{/each}}
{{/each}}
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
{{#each JoinNonCdcChildren}}
      {{JoinTypeSql}} [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
{{/each}}
{{#if IdentifierMapping}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.IdentifierMappingTable}}] AS [_im] ON ([_im].[Schema] = '{{Schema}}' AND [_im].[Table] = '{{Name}}' AND [_im].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
{{/if}}
{{#each SelectedColumnsExcludingPrimaryKey}}
  {{#ifval IdentifierMappingParent}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.IdentifierMappingTable}}] AS [{{IdentifierMappingAlias}}] ON ([{{IdentifierMappingAlias}}].[Schema] = '{{IdentifierMappingSchema}}' AND [{{IdentifierMappingAlias}}].[Table] = '{{IdentifierMappingTable}}' AND [{{IdentifierMappingAlias}}].[Key] = CAST([{{IdentifierMappingParent.Parent.Alias}}].[{{IdentifierMappingParent.Name}}] AS NVARCHAR(128))) 
  {{/ifval}}
{{/each}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.VersionTrackingTable}}] AS [_ct] ON ([_ct].[Schema] = '{{Schema}}' AND [_ct].[Table] = '{{Name}}' AND [_ct].[Key] = {{#if IdentifierMapping}}_im.GlobalId){{else}}{{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([_chg].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}{{/if}}
      ORDER BY [_Lsn] ASC

{{#each CdcJoins}}
    -- Related table: '[{{Schema}}].[{{Table}}]' - unique name '{{Name}}' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
  {{#if IdentifierMapping}}
        [_im].[GlobalId] AS [GlobalId],
  {{/if}}
  {{#each JoinHierarchyReverse}}
    {{#unless @last}}
      {{#each OnSelectColumns}}
        [{{Parent.JoinToAlias}}].[{{ToColumn}}] AS [{{pascal Parent.JoinTo}}_{{Name}}],  -- Additional joining column (informational).
      {{/each}}
    {{/unless}}
  {{/each}}
  {{#each Columns}}
        [{{#ifval IdentifierMappingAlias}}{{IdentifierMappingAlias}}{{else}}{{Parent.Alias}}{{/ifval}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#ifne Parent.JoinNonCdcChildren.Count 0}},{{/ifne}}{{/unless}}
  {{/each}}
  {{#each JoinNonCdcChildren}}
    {{#each Columns}}
        [{{Parent.Alias}}].[{{Name}}] AS [{{NameAlias}}]{{#unless @last}},{{else}}{{#unless @../last}},{{/unless}}{{/unless}}
    {{/each}}
  {{/each}}
      FROM #_changes AS [_chg]
      INNER JOIN [{{Parent.Schema}}].[{{Parent.Table}}] AS [{{Parent.Alias}}] ON ({{#each Parent.PrimaryKeyColumns}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = [_chg].[{{Name}}]{{/each}})
  {{#each JoinHierarchyReverse}}
      INNER JOIN [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
  {{#each JoinNonCdcChildren}}
      {{JoinTypeSql}} [{{Schema}}].[{{Table}}] AS [{{Alias}}] ON ({{#each On}}{{#unless @first}} AND {{/unless}}[{{Parent.Alias}}].[{{Name}}] = {{#ifval ToStatement}}{{ToStatement}}{{else}}[{{Parent.JoinToAlias}}].[{{ToColumn}}]{{/ifval}}{{/each}})
  {{/each}}
  {{#if IdentifierMapping}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.IdentifierMappingTable}}] AS [_im] ON ([_im].[Schema] = '{{Schema}}' AND [_im].[Table] = '{{Name}}' AND [_im].[Key] = {{#ifeq PrimaryKeyColumns.Count 1}}{{#each PrimaryKeyColumns}}CAST([{{Parent.Alias}}].[{{Name}}] AS NVARCHAR(128))){{/each}}{{else}}CONCAT({{#each PrimaryKeyColumns}}CAST([{{Parent.Alias}}].[{{Name}}] AS NVARCHAR(128))){{#unless @last}}, ',', {{/unless}}{{/each}}){{/ifeq}}
  {{/if}}
  {{#each Columns}}
    {{#ifval IdentifierMappingParent}}
      LEFT OUTER JOIN [{{Root.CdcSchema}}].[{{Root.IdentifierMappingTable}}] AS [{{IdentifierMappingAlias}}] ON ([{{IdentifierMappingAlias}}].[Schema] = '{{IdentifierMappingSchema}}' AND [{{IdentifierMappingAlias}}].[Table] = '{{IdentifierMappingTable}}' AND [{{IdentifierMappingAlias}}].[Key] = CAST([{{IdentifierMappingParent.Parent.Alias}}].[{{IdentifierMappingParent.Name}}] AS NVARCHAR(128))) 
    {{/ifval}}
  {{/each}}
      WHERE [_chg].[_Op] <> 1

{{/each}}
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