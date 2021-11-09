// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using System;

namespace NTangle.Data
{
    /// <summary>
    /// Represents a known Database Error <see cref="Exception"/>; i.e. message that should be reported back to consumer.
    /// </summary>
    /// <remarks>As opposed to an unexpected <see cref="Exception"/>.</remarks>
    public class DatabaseErrorException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseErrorException"/> class.
        /// </summary>
        public DatabaseErrorException() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseErrorException"/> class with a specified message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DatabaseErrorException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseErrorException"/> class with a specified messsage and inner exception.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="innerException">The inner <see cref="Exception"/>.</param>
        public DatabaseErrorException(string message, Exception innerException) : base(message, innerException) { }
    }
}