/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace SqlServerDemo.Publisher.Data;

/// <summary>
/// Enables the Change Data Capture (CDC) <see cref="PostCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Posts]').
/// </summary>
public partial interface IPostOrchestrator : IEntityOrchestrator<PostCdc> { }

/// <summary>
/// Manages the Change Data Capture (CDC) <see cref="PostCdc"/> entity (aggregate root) orchestration (database table '[Legacy].[Posts]').
/// </summary>
public partial class PostOrchestrator : EntityOrchestrator<PostCdc, PostOrchestrator.PostCdcEnvelopeCollection, PostOrchestrator.PostCdcEnvelope, VersionTrackingMapper>, IPostOrchestrator
{
    private static readonly PostCdcMapper _postCdcMapper = new();
    private static readonly CommentCdcMapper _commentCdcMapper = new();
    private static readonly CommentsTagsCdcMapper _commentsTagsCdcMapper = new();
    private static readonly PostsTagsCdcMapper _postsTagsCdcMapper = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PostOrchestrator"/> class.
    /// </summary>
    /// <param name="db">The <see cref="IDatabase"/>.</param>
    /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
    /// <param name="jsonSerializer">The <see cref="IJsonSerializer"/>.</param>
    /// <param name="settings">The <see cref="SettingsBase"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    public PostOrchestrator(IDatabase db, IEventPublisher eventPublisher, IJsonSerializer jsonSerializer, SettingsBase settings, ILogger<PostOrchestrator> logger) :
        base(db, "[NTangle].[spPostsBatchExecute]", "[NTangle].[spPostsBatchComplete]", eventPublisher, jsonSerializer, settings, logger) => PostOrchestratorCtor();

    partial void PostOrchestratorCtor(); // Enables additional functionality to be added to the constructor.

    /// <inheritdoc/>
    protected override async Task<EntityOrchestratorResult<PostCdcEnvelopeCollection, PostCdcEnvelope>> GetBatchEntityDataAsync(CancellationToken cancellationToken = default)
    {
        var pColl = new PostCdcEnvelopeCollection();

        var result = await SelectQueryMultiSetAsync(MultiSetArgs.Create(
            // Root table: '[Legacy].[Posts]'
            new MultiSetCollArgs<PostCdcEnvelopeCollection, PostCdcEnvelope>(_postCdcMapper, __result => pColl = __result, stopOnNull: true),

            // Join table: '[Legacy].[Comments]' (unique name 'Comments')
            new MultiSetCollArgs<PostCdc.CommentCdcCollection, PostCdc.CommentCdc>(_commentCdcMapper, __result =>
            {
                foreach (var c in __result.GroupBy(x => new { x.PostsId }).Select(g => new { g.Key.PostsId, Coll = g.ToCollection<PostCdc.CommentCdcCollection, PostCdc.CommentCdc>() }))
                {
                    pColl.Where(x => x.PostsId == c.PostsId).ForEach(x => x.Comments = c.Coll);
                }
            }),

            // Join table: '[Legacy].[Tags]' (unique name 'CommentsTags')
            new MultiSetCollArgs<PostCdc.CommentsTagsCdcCollection, PostCdc.CommentsTagsCdc>(_commentsTagsCdcMapper, __result =>
            {
                foreach (var c in __result.GroupBy(x => new { x.Posts_PostsId }).Select(g => new { g.Key.Posts_PostsId, Coll = g.ToList() }))
                {
                    var pItem = pColl.First(x => x.PostsId == c.Posts_PostsId).Comments ?? [];
                    foreach (var ct in c.Coll.GroupBy(x => new { x.ParentId }).Select(g => new { g.Key.ParentId, Coll = g.ToCollection<PostCdc.CommentsTagsCdcCollection, PostCdc.CommentsTagsCdc>() }))
                    {
                        pItem.Where(x => x.CommentsId == ct.ParentId).ForEach(x => x.Tags = ct.Coll);
                    }
                }
            }),

            // Join table: '[Legacy].[Tags]' (unique name 'PostsTags')
            new MultiSetCollArgs<PostCdc.PostsTagsCdcCollection, PostCdc.PostsTagsCdc>(_postsTagsCdcMapper, __result =>
            {
                foreach (var pt in __result.GroupBy(x => new { x.PostsId }).Select(g => new { g.Key.PostsId, Coll = g.ToCollection<PostCdc.PostsTagsCdcCollection, PostCdc.PostsTagsCdc>() }))
                {
                    pColl.Where(x => x.PostsId == pt.PostsId).ForEach(x => x.Tags = pt.Coll);
                }
            })), cancellationToken).ConfigureAwait(false);

        result.Result.AddRange(pColl);
        return result;
    }

    /// <inheritdoc/>
    protected override string EventSubject => "Legacy.Post";

    /// <inheritdoc/>
    protected override EventSubjectFormat EventSubjectFormat => EventSubjectFormat.NameAndKey;

    /// <inheritdoc/>
    protected override EventActionFormat EventActionFormat => EventActionFormat.PastTense;

    /// <inheritdoc/>
    protected override string? EventType => "Legacy.Post";

    /// <inheritdoc/>
    protected override Uri? EventSource => new("/database/cdc/legacy/posts", UriKind.Relative);

    /// <inheritdoc/>
    protected override EventSourceFormat EventSourceFormat { get; } = EventSourceFormat.NameOnly;

    /// <summary>
    /// Represents a <see cref="PostCdc"/> envelope to append the required (additional) database properties.
    /// </summary>
    public class PostCdcEnvelope : PostCdc, IEntityEnvelope
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public CdcOperationType DatabaseOperationType { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public byte[] DatabaseLsn { get; set; } = [];

        /// <inheritdoc/>
        [JsonIgnore]
        public string? DatabaseTrackingHash { get; set; }

        /// <inheritdoc/>
        [JsonIgnore]
        public bool IsDatabasePhysicallyDeleted { get; set; }
    }

    /// <summary>
    /// Represents a <see cref="PostCdcEnvelope"/> collection.
    /// </summary>
    public class PostCdcEnvelopeCollection : List<PostCdcEnvelope> { }

    /// <summary>
    /// Represents a <see cref="PostCdc"/> database mapper.
    /// </summary>
    public class PostCdcMapper : IDatabaseMapper<PostCdcEnvelope>
    {
        /// <inheritdoc/>
        public PostCdcEnvelope? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            PostsId = record.GetValue<int>("PostsId"),
            Text = record.GetValue<string?>("Text"),
            Date = record.GetValue<DateTime?>("Date"),
            DatabaseOperationType = record.GetValue<CdcOperationType>(CdcOperationTypeColumnName),
            DatabaseLsn = record.GetValue<byte[]>(CdcLsnColumnName),
            DatabaseTrackingHash = record.GetValue<string?>(TrackingHashColumnName),
            IsDatabasePhysicallyDeleted = record.GetValue<bool>(IsPhysicallyDeletedColumnName)
        };

        /// <inheritdoc/>
        void IDatabaseMapper<PostCdcEnvelope>.MapToDb(PostCdcEnvelope? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a <see cref="CommentCdc"/> database mapper.
    /// </summary>
    public class CommentCdcMapper : IDatabaseMapper<PostCdc.CommentCdc>
    {
        /// <inheritdoc/>
        public PostCdc.CommentCdc? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            CommentsId = record.GetValue<int>("CommentsId"),
            PostsId = record.GetValue<int>("PostsId"),
            Text = record.GetValue<string?>("Text"),
            Date = record.GetValue<DateTime?>("Date")
        };

        /// <inheritdoc/>
        void IDatabaseMapper<PostCdc.CommentCdc>.MapToDb(PostCdc.CommentCdc? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a <see cref="CommentsTagsCdc"/> database mapper.
    /// </summary>
    public class CommentsTagsCdcMapper : IDatabaseMapper<PostCdc.CommentsTagsCdc>
    {
        /// <inheritdoc/>
        public PostCdc.CommentsTagsCdc? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            Posts_PostsId = record.GetValue<int>("Posts_PostsId"),
            TagsId = record.GetValue<int>("TagsId"),
            ParentType = record.GetValue<string?>("ParentType"),
            ParentId = record.GetValue<int>("ParentId"),
            Text = record.GetValue<string?>("Text")
        };

        /// <inheritdoc/>
        void IDatabaseMapper<PostCdc.CommentsTagsCdc>.MapToDb(PostCdc.CommentsTagsCdc? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }

    /// <summary>
    /// Represents a <see cref="PostsTagsCdc"/> database mapper.
    /// </summary>
    public class PostsTagsCdcMapper : IDatabaseMapper<PostCdc.PostsTagsCdc>
    {
        /// <inheritdoc/>
        public PostCdc.PostsTagsCdc? MapFromDb(DatabaseRecord record, OperationTypes operationType) => new()
        {
            TagsId = record.GetValue<int>("TagsId"),
            ParentType = record.GetValue<string?>("ParentType"),
            PostsId = record.GetValue<int>("PostsId"),
            Text = record.GetValue<string?>("Text")
        };

        /// <inheritdoc/>
        void IDatabaseMapper<PostCdc.PostsTagsCdc>.MapToDb(PostCdc.PostsTagsCdc? value, DatabaseParameterCollection parameters, OperationTypes operationType) => throw new NotImplementedException();
    }
}