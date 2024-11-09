BEGIN
  /*
   * This is automatically generated; any changes will be lost.
   */

  SET NOCOUNT ON;

  BEGIN TRY
    -- Declare variables.
    DECLARE @lsn BINARY(10)

    CREATE TABLE #_changes ([_Lsn] BINARY(10), [_Op] INT, [PostsId] INT)

    -- Simulate changes on the root table: '[Legacy].[Posts]'.
    IF @PostsKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #PostsKeysList FROM OPENJSON(@PostsKeysList) WITH ([PostsId] INT '$.PostsId')

      INSERT INTO #_changes
        SELECT
            @lsn AS [_Lsn],
            4 AS [_Op],
            [_cdc].[PostsId] AS [PostsId]
          FROM #PostsKeysList AS [_cdc]
    END

    -- Simulate changes on the related table: '[Legacy].[Comments]'.
    IF @CommentsKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #CommentsKeysList FROM OPENJSON(@CommentsKeysList) WITH ([CommentsId] INT '$.CommentsId')

      SELECT
          @lsn AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #c
        FROM #CommentsKeysList AS [_cdc]
        INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([c].[CommentsId] = [_cdc].[CommentsId])
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])

      IF (@@ROWCOUNT <> 0)
      BEGIN
        INSERT INTO #_changes
          SELECT * 
            FROM #c AS [_c]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_c].[PostsId])
      END
    END

    -- Simulate changes on the related table: '[Legacy].[Tags]'.
    IF @CommentsTagsKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #CommentsTagsKeysList FROM OPENJSON(@CommentsTagsKeysList) WITH ([TagsId] INT '$.TagsId')

      SELECT
          @lsn AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #ct
        FROM #CommentsTagsKeysList AS [_cdc]
        INNER JOIN [Legacy].[Tags] AS [ct] WITH (NOLOCK) ON ([ct].[TagsId] = [_cdc].[TagsId] AND [ct].[ParentType] = 'C')
        INNER JOIN [Legacy].[Comments] AS [c] WITH (NOLOCK) ON ([ct].[ParentId] = [c].[CommentsId])
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([c].[PostsId] = [p].[PostsId])

      IF (@@ROWCOUNT <> 0)
      BEGIN
        INSERT INTO #_changes
          SELECT * 
            FROM #ct AS [_ct]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_ct].[PostsId])
      END
    END

    -- Simulate changes on the related table: '[Legacy].[Tags]'.
    IF @PostsTagsKeysList IS NOT NULL
    BEGIN
      SELECT * INTO #PostsTagsKeysList FROM OPENJSON(@PostsTagsKeysList) WITH ([TagsId] INT '$.TagsId')

      SELECT
          @lsn AS [_Lsn],
          4 AS [_Op],
          [p].[PostsId] AS [PostsId]
        INTO #pt
        FROM #PostsTagsKeysList AS [_cdc]
        INNER JOIN [Legacy].[Tags] AS [pt] WITH (NOLOCK) ON ([pt].[TagsId] = [_cdc].[TagsId] AND [pt].[ParentType] = 'P')
        INNER JOIN [Legacy].[Posts] AS [p] WITH (NOLOCK) ON ([pt].[ParentId] = [p].[PostsId])

      IF (@@ROWCOUNT <> 0)
      BEGIN
        INSERT INTO #_changes
          SELECT * 
            FROM #pt AS [_pt]
            WHERE NOT EXISTS (SELECT * FROM #_changes AS [_chg] WHERE [_chg].[PostsId] = [_pt].[PostsId])
      END
    END

    -- Root table: '[Legacy].[Posts]' - uses LEFT OUTER JOIN's to get the deleted records.
    SELECT
        [_chg].[_Op] AS [_OperationType],
        [_chg].[_Lsn] AS [_Lsn],
        [_chg].[PostsId] AS [PostsId],
        [p].[PostsId] AS [TableKey_PostsId],
        [p].[Text] AS [Text],
        [p].[Date] AS [Date],
        CASE WHEN EXISTS (SELECT 1 FROM [Legacy].[Posts] AS [__p] WHERE ([__p].[PostsId] = [_chg].[PostsId])) THEN CAST (0 AS BIT) ELSE CAST (1 AS BIT) END AS [_IsPhysicallyDeleted]
      FROM #_changes AS [_chg]
      LEFT OUTER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])

    -- Related table: '[Legacy].[Comments]' - unique name 'Comments' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [c].[CommentsId] AS [CommentsId],
        [c].[PostsId] AS [PostsId],
        [c].[Text] AS [Text],
        [c].[Date] AS [Date]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Comments] AS [c] ON ([c].[PostsId] = [p].[PostsId])

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

    -- Related table: '[Legacy].[Tags]' - unique name 'PostsTags' - only use INNER JOINS to get what is actually there right now (where applicable).
    SELECT DISTINCT
        [pt].[TagsId] AS [TagsId],
        [pt].[ParentType] AS [ParentType],
        [pt].[ParentId] AS [PostsId],
        [pt].[Text] AS [Text]
      FROM #_changes AS [_chg]
      INNER JOIN [Legacy].[Posts] AS [p] ON ([p].[PostsId] = [_chg].[PostsId])
      INNER JOIN [Legacy].[Tags] AS [pt] ON ([pt].[ParentType] = 'P' AND [pt].[ParentId] = [p].[PostsId])

    RETURN
  END TRY
  BEGIN CATCH
    -- Rollback transaction and rethrow error.
    IF @@TRANCOUNT > 0
      ROLLBACK TRANSACTION;

    THROW;
  END CATCH
END