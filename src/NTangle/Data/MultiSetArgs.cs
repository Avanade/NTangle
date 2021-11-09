// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;
using System.Collections.Generic;

namespace NTangle.Data
{
    /// <summary>
    /// Enables the <b>Database</b> multi-set arguments
    /// </summary>
    public interface IMultiSetArgs
    {
        /// <summary>
        /// Gets the minimum number of rows allowed.
        /// </summary>
        int MinRows { get; }

        /// <summary>
        /// Gets the maximum number of rows allowed.
        /// </summary>
        int? MaxRows { get; }

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        bool StopOnNull { get; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        void InvokeResult();
    }

    /// <summary>
    /// Enables the <b>Database</b> multi-set arguments with a <see cref="Mapper"/>.
    /// </summary>
    /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
    public interface IMultiSetArgs<T> : IMultiSetArgs
        where T : class, new()
    {
        /// <summary>
        /// Gets the <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        IDatabaseMapper<T> Mapper { get; }
    }

    /// <summary>
    /// Provides the base <b>Database</b> multi-set arguments when expecting a single item/record only.
    /// </summary>
    public abstract class MultiSetSingleArgs : IMultiSetArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetSingleArgs"/> class.
        /// </summary>
        /// <param name="isMandatory">Indicates whether the value is mandatory; defaults to <c>true</c>.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        protected MultiSetSingleArgs(bool isMandatory = true, bool stopOnNull = false)
        {
            IsMandatory = isMandatory;
            StopOnNull = stopOnNull;
        }

        /// <summary>
        /// Indicates whether the value is mandatory; i.e. a corresponding record must be read.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of rows allowed.
        /// </summary>
        public int MinRows => IsMandatory ? 1 : 0;

        /// <summary>
        /// Gets or sets the maximum number of rows allowed.
        /// </summary>
        public int? MaxRows => 1;

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        public bool StopOnNull { get; set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public abstract void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public virtual void InvokeResult() { }
    }

    /// <summary>
    /// Provides the <b>Database</b> multi-set arguments when expecting a single item/record only.
    /// </summary>
    /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
    public class MultiSetSingleArgs<T> : MultiSetSingleArgs, IMultiSetArgs<T>
        where T : class, new()
    {
        private T? _value;
        private readonly Action<T> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetSingleArgs{TItem}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.</param>
        /// <param name="result">The action that will be invoked with the result of the set.</param>
        /// <param name="isMandatory">Indicates whether the value is mandatory; defaults to <c>true</c>.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        public MultiSetSingleArgs(IDatabaseMapper<T> mapper, Action<T> result, bool isMandatory = true, bool stopOnNull = false) : base(isMandatory, stopOnNull)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets the <see cref="IDatabaseMapper{T}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        public IDatabaseMapper<T> Mapper { get; private set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public override void DatasetRecord(DatabaseRecord dr) => _value = Mapper.MapFromDb(dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public override void InvokeResult()
        {
            if (_value != null)
                _result(_value);
        }
    }

    /// <summary>
    /// Provides the base <b>Database</b> multi-set arguments when expecting a collection of items/records.
    /// </summary>
    public abstract class MultiSetCollArgs : IMultiSetArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetCollArgs"/> class.
        /// </summary>
        /// <param name="minRows">The minimum number of rows allowed.</param>
        /// <param name="maxRows">The maximum number of rows allowed.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        protected MultiSetCollArgs(int minRows = 0, int? maxRows = null, bool stopOnNull = false)
        {
            if (maxRows.HasValue && minRows <= maxRows.Value)
                throw new ArgumentException("Max Rows is less than Min Rows.", nameof(maxRows));

            MinRows = minRows;
            MaxRows = maxRows;
            StopOnNull = stopOnNull;
        }

        /// <summary>
        /// Gets or sets the minimum number of rows allowed.
        /// </summary>
        public int MinRows { get; }

        /// <summary>
        /// Gets or sets the maximum number of rows allowed.
        /// </summary>
        public int? MaxRows { get; }

        /// <summary>
        /// Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).
        /// </summary>
        public bool StopOnNull { get; set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public abstract void DatasetRecord(DatabaseRecord dr);

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public virtual void InvokeResult() { }
    }

    /// <summary>
    /// Provides the <b>Database</b> multi-set arguments when expecting a collection of items/records.
    /// </summary>
    /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
    public class MultiSetCollArgs<TColl, TItem> : MultiSetCollArgs, IMultiSetArgs<TItem>
        where TItem : class, new()
        where TColl : class, ICollection<TItem>, new()
    {
        private TColl? _coll;
        private readonly Action<TColl> _result;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiSetCollArgs{TColl, TItem}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.</param>
        /// <param name="result">The action that will be invoked with the result of the set.</param>
        /// <param name="minRows">The minimum number of rows allowed.</param>
        /// <param name="maxRows">The maximum number of rows allowed.</param>
        /// <param name="stopOnNull">Indicates whether to stop further query result set processing where the current set has resulted in a null (i.e. no records).</param>
        public MultiSetCollArgs(IDatabaseMapper<TItem> mapper, Action<TColl> result, int minRows = 0, int? maxRows = null, bool stopOnNull = false) : base(minRows, maxRows, stopOnNull)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>
        /// Gets the <see cref="IDatabaseMapper{TItem}"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        public IDatabaseMapper<TItem> Mapper { get; private set; }

        /// <summary>
        /// The <see cref="DatabaseRecord"/> method invoked for each record for its respective dataset.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public override void DatasetRecord(DatabaseRecord dr)
        {
            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            if (_coll == null)
                _coll = new TColl();

            var item = Mapper.MapFromDb(dr);
            if (item != null)
                _coll.Add(item);
        }

        /// <summary>
        /// Invokes the corresponding result function.
        /// </summary>
        public override void InvokeResult()
        {
            if (_coll != null)
                _result(_coll);
        }
    }
}