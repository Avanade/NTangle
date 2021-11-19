CREATE PROCEDURE [NTangle].[spPostsBatchExecute]
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
    DECLARE @PostsBaseMinLsn BINARY(10), @PostsMinLsn BINARY(10), @PostsMaxLsn BINARY(10)
    DECLARE @CommentsBaseMinLsn BINARY(10), @CommentsMinLsn BINARY(10), @CommentsMaxLsn BINARY(10)
    DECLARE @CommentsTagsBaseMinLsn BINARY(10), @CommentsTagsMinLsn BINARY(10), @CommentsTagsMaxLsn BINARY(10)
    DECLARE @PostsTagsBaseMinLsn BINARY(10), @PostsTagsMinLsn BINARY(10), @PostsTagsMaxLsn BINARY(10)
    DECLARE @BatchTrackingId INT

    -- Check if there is already an incomplete batch and attempt to reprocess.
    SELECT
        @PostsMinLsn = [_batch].[PostsMinLsn],
        @PostsMaxLsn = [_batch].[PostsMaxLsn],
        @CommentsMinLsn = [_batch].[CommentsMinLsn],
        @CommentsMaxLsn = [_batch].[CommentsMaxLsn],
        @CommentsTagsMinLsn = [_batch].[CommentsTagsMinLsn],
        @CommentsTagsMaxLsn = [_batch].[CommentsTagsMaxLsn],
        @PostsTagsMinLsn = [_batch].[PostsTagsMinLsn],
        @PostsTagsMaxLsn = [_batch].[PostsTagsMaxLsn],
        @BatchTrackingId = [BatchTrackingId]
      FROM [NTangle].[PostsBatchTracking] AS [_batch]
      WHERE [_batch].[IsComplete] = 0 
      ORDER BY [_batch].[BatchTrackingId]

    -- There should never be more than one incomplete batch.
    IF @@ROWCOUNT > 1
    BEGIN
      ;THROW 56002, 'There are multiple incomplete batches; there should not be more than one incomplete batch at any one time.', 1
    END

    -- Get the latest 'base' minimum.
    SET @PostsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Posts');
    SET @CommentsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Comments');
    SET @CommentsTagsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');
    SET @PostsTagsBaseMinLsn = sys.fn_cdc_get_min_lsn('Legacy_Tags');

    -- Where there is no incomplete batch then the next should be created/processed.
    IF (@BatchTrackingId IS NULL)
    BEGIN
      -- Get the last batch processed.
      SELECT TOP 1
          @PostsMinLsn = [_batch].[PostsMaxLsn],
          @CommentsMinLsn = [_batch].[CommentsMaxLsn],
          @CommentsTagsMinLsn = [_batch].[CommentsTagsMaxLsn],
          @PostsTagsMinLsn = [_batch].[PostsTagsMaxLsn]
        FROM [NTangle].[PostsBatchTracking] AS [_batch]
        ORDER BY [_batch].[IsComplete] ASC, [_batch].[BatchTrackingId] DESC

      IF (@@ROWCOUNT = 0) -- No previous batch; i.e. is the first time!
      BEGIN
        SET @PostsMinLsn = @PostsBaseMinLsn;
        SET @CommentsMinLsn = @CommentsBaseMinLsn;
        SET @CommentsTagsMinLsn = @CommentsTagsBaseMinLsn;
        SET @PostsTagsMinLsn = @PostsTagsBaseMinLsn;
      END
      ELSE
      BEGIN
        -- Increment the minimum as the last has already been processed.
        SET @PostsMinLsn = sys.fn_cdc_increment_lsn(@PostsMinLsn)
        SET @CommentsMinLsn = sys.fn_cdc_increment_lsn(@CommentsMinLsn)
        SET @CommentsTagsMinLsn = sys.fn_cdc_increment_lsn(@CommentsTagsMinLsn)
        SET @PostsTagsMinLsn = sys.fn_cdc_increment_lsn(@PostsTagsMinLsn)
      END

      -- Get the maximum LSN.
      SET @PostsMaxLsn = sys.fn_cdc_get_max_lsn();
      SET @CommentsMaxLsn = @PostsMaxLsn
      SET @CommentsTagsMaxLsn = @PostsMaxLsn
      SET @PostsTagsMaxLsn = @PostsMaxLsn

      -- Verify the maximum query size and correct (reset) where applicable.
      IF (@MaxQuerySize IS NULL OR @MaxQuerySize < 1 OR @MaxQuerySize > 10000)
      BEGIN
        SET @MaxQuerySize = 100
      END
    END

    -- The minimum should _not_ be less than the base otherwise we have lost data; either continue with this data loss, or error and stop.
    DECLARE @hasDataLoss BIT = 0

    IF (@PostsMinLsn < @PostsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PostsMinLsn = @PostsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Posts''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@CommentsMinLsn < @CommentsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CommentsMinLsn = @CommentsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Comments''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@CommentsTagsMinLsn < @CommentsTagsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @CommentsTagsMinLsn = @CommentsTagsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Tags''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END
    IF (@PostsTagsMinLsn < @PostsTagsBaseMinLsn) BEGIN IF (@ContinueWithDataLoss = 1) BEGIN SET @hasDataLoss = 1; SET @PostsTagsMinLsn = @PostsTagsBaseMinLsn END ELSE BEGIN ;THROW 56002, 'Unexpected data loss error for ''Legacy.Tags''; this indicates that the CDC data has probably been cleaned up before being successfully processed.', 1; END END

    -- Find changes on the root table: '[Legacy].[Posts]' - this determines overall operation type: 'create', 'update' or 'delete'.
    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [PostsId] INT)
    DECLARE @hasChanges BIT = 0

    IF (@PostsMinLsn <= @PostsMaxLsn)
    BEGIN
      INSERT INTO #_changes
        SELECT TOP (@MaxQuerySize)
            [_cdc].[__$start_lsn] AS [_Lsn],
            [_cdc].[__$operation] AS [_Op],
            [_cdc].[PostsId] AS [PostsId]
          FROM cdc.fn_cdc_get_all_changes_Legacy_Posts(@PostsMinLsn, @PostsMaxLsn, 'all') AS [_cdc]
          ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PostsMinLsn = MIN([_Lsn]), @PostsMaxLsn = MAX([_Lsn]) FROM #_changes
      END
    END

    -- Find changes on related table: '[Legacy].[Comments]' - unique name 'Comments' - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@CommentsMinLsn <= @CommentsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #c
        FROM cdc.fn_cdc_get_all_changes_Legacy_Comments(@CommentsMinLsn, @CommentsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[PostsId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CommentsMinLsn = MIN([_Lsn]), @CommentsMaxLsn = MAX([_Lsn]) FROM #c

        INSERT INTO #_changes
          SELECT * 
            FROM #c AS [_c]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_c].[PostsId])
      END
    END

    -- Find changes on related table: '[Legacy].[Tags]' - unique name 'CommentsTags' - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@CommentsTagsMinLsn <= @CommentsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #ct
        FROM cdc.fn_cdc_get_all_changes_Legacy_Tags(@CommentsTagsMinLsn, @CommentsTagsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'C' AND [_cdc].[ParentId] = [c].[CommentsId])
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @CommentsTagsMinLsn = MIN([_Lsn]), @CommentsTagsMaxLsn = MAX([_Lsn]) FROM #ct

        INSERT INTO #_changes
          SELECT * 
            FROM #ct AS [_ct]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_ct].[PostsId])
      END
    END

    -- Find changes on related table: '[Legacy].[Tags]' - unique name 'PostsTags' - assume all are 'update' operation (i.e. it doesn't matter).
    IF (@PostsTagsMinLsn <= @PostsTagsMaxLsn)
    BEGIN
      SELECT TOP (@MaxQuerySize)
          [_cdc].[__$start_lsn] AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #pt
        FROM cdc.fn_cdc_get_all_changes_Legacy_Tags(@PostsTagsMinLsn, @PostsTagsMaxLsn, 'all') AS [_cdc]
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([_cdc].[ParentType] = 'P' AND [_cdc].[ParentId] = [p].[PostsId])
        ORDER BY [_cdc].[__$start_lsn]

      IF (@@ROWCOUNT <> 0)
      BEGIN
        SET @hasChanges = 1
        SELECT @PostsTagsMinLsn = MIN([_Lsn]), @PostsTagsMaxLsn = MAX([_Lsn]) FROM #pt

        INSERT INTO #_changes
          SELECT * 
            FROM #pt AS [_pt]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_pt].[PostsId])
      END
    END

    -- Create a new batch where not processing an existing.
    IF (@BatchTrackingId IS NULL AND (@hasDataLoss = 1 OR @hasChanges = 1))
    BEGIN
      DECLARE @InsertedBatchTrackingId TABLE([BatchTrackingId] INT)

      INSERT INTO [NTangle].[PostsBatchTracking] (
          [PostsMinLsn],
          [PostsMaxLsn],
          [CommentsMinLsn],
          [CommentsMaxLsn],
          [CommentsTagsMinLsn],
          [CommentsTagsMaxLsn],
          [PostsTagsMinLsn],
          [PostsTagsMaxLsn],
          [CreatedDate],
          [IsComplete],
          [CorrelationId],
          [HasDataLoss]
        ) 
        OUTPUT inserted.BatchTrackingId INTO @InsertedBatchTrackingId
        VALUES (
          @PostsMinLsn,
          @PostsMaxLsn,
          @CommentsMinLsn,
          @CommentsMaxLsn,
          @CommentsTagsMinLsn,
          @CommentsTagsMaxLsn,
          @PostsTagsMinLsn,
          @PostsTagsMaxLsn,
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
        UPDATE [NTangle].[PostsBatchTracking] 
          SET [HasDataLoss] = @hasDataLoss
          WHERE [BatchTrackingId] = @BatchTrackingId
      END
    END

    -- Return the *latest* batch tracking data.
    SELECT [_batch].[BatchTrackingId], [_batch].[CreatedDate], [_batch].[IsComplete], [_batch].[CompletedDate], [_batch].[CorrelationId], [_batch].[HasDataLoss]
      FROM [NTangle].[PostsBatchTracking] AS [_batch]
      WHERE [_batch].BatchTrackingId = @BatchTrackingId 

    -- Exit here if there were no changes found.
    IF (@hasChanges = 0)
    BEGIN
      COMMIT TRANSACTION
      RETURN 0
    END

    -- Root table: '[Legacy].[Posts]' - uses LEFT OUTER JOIN's to get the deleted records, as well as any previous 'TrackingHash' value.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_ct].[Hash] AS [_TrackingHash],
        [_chg].[PostsId] AS [PostsId],
        [p].[PostsId] AS [TableKey_PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      LEFT OUTER JOIN [NTangle].[VersionTracking] AS [_ct] ON ([_ct].[Schema] = 'Legacy' AND [_ct].[Table] = 'Posts' AND [_ct].[Key] = CAST([_chg].[PostsId] AS NVARCHAR(128)))
      ORDER BY [_Lsn] ASC

    -- Related table: '[Legacy].[Comments]' - unique name 'Comments' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: '[Legacy].[Tags]' - unique name 'CommentsTags' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [p].[PostsId] AS [Posts_PostsId],  -- Additional joining column (informational).
        [ct].[TagsId] AS [TagsId],
        [ct].[ParentType] AS [ParentType],
        [ct].[ParentId] AS [ParentId],
        [ct].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [ct] ON ([ct].[ParentType] = 'C' AND [ct].[ParentId] = [c].[CommentsId])
      WHERE [_chg].[_Op] <> 1

    -- Related table: '[Legacy].[Tags]' - unique name 'PostsTags' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [pt].[TagsId] AS [TagsId],
        [pt].[ParentType] AS [ParentType],
        [pt].[ParentId] AS [PostsId],
        [pt].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [pt] ON ([pt].[ParentType] = 'P' AND [pt].[ParentId] = [p].[PostsId])
      WHERE [_chg].[_Op] <> 1

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