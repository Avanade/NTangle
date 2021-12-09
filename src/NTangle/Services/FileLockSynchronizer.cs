﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace NTangle.Services
{
    /// <summary>
    /// An <see cref="IServiceSynchronizer"/> that performs synchronization by taking an exclusive lock on a file.
    /// </summary>
    /// <remarks>A lock file is created per <see cref="Type"/> with a name of <see cref="Type.FullName"/> and extension of '.lock'; e.g. '<c>Namespace.Class.lock</c>'. For this to function corrently all running
    /// instances must be referencing the same shared directory as specified by the <see cref="ConfigKey"/> (<see cref="IConfiguration"/>).</remarks>
    public sealed class FileLockSynchronizer : IServiceSynchronizer
    {
        /// <summary>
        /// Gets the configuration key that defines the directory path for the exclusive lock files.
        /// </summary>
        public const string ConfigKey = "FileLockHostedServiceSynchronizePath";

        private readonly string _path;
        private readonly ConcurrentDictionary<string, FileStream> _dict = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLockSynchronizer"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public FileLockSynchronizer(IConfiguration config)
        {
            _path = (config ?? throw new ArgumentNullException(nameof(config))).GetValue<string>(ConfigKey);
            if (string.IsNullOrEmpty(_path))
                throw new ArgumentException($"Configuration setting '{ConfigKey}' either does not exist or has no value.", nameof(config));

            if (!Directory.Exists(_path))
                throw new ArgumentException($"Configuration setting '{ConfigKey}' path does not exist: {_path}");
        }

        /// <inheritdoc/>
        public bool Enter<T>(string? name = null)
        {
            var fn = Path.Combine(_path, $"{typeof(T).FullName}{(name == null ? "" : $".{name}")}.lock");

            try
            {
                // Is exclusive for this invocation only where genuinely creating.
                bool exclusiveLock = false;
                _dict.GetOrAdd(GetName<T>(name), _ => { exclusiveLock = true; return File.Create(fn, 1, FileOptions.DeleteOnClose); });
                return exclusiveLock;
            }
            catch (IOException) { return false; } // Already exists and locked!
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected exception whilst attemptiong to create file '{fn}' with an exclusive lock: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public void Exit<T>(string? name)
        {
            if (_dict.TryRemove(GetName<T>(name), out var fs))
                fs.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose() => _dict.Values.ForEach(fs => fs.Dispose());

        /// <summary>
        /// Gets the full name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetName<T>(string? name) => $"{typeof(T).FullName}{(name == null ? "" : $".{name}")}";
    }
}