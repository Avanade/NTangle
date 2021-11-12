// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/NTangle

using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace NTangle.Events
{
    /// <summary>
    /// Represents as <see cref="ILogger"/> event publisher; whereby the events are output using <see cref="LoggerExtensions.LogInformation(ILogger, string, object[])"/>.
    /// </summary>
    public class LoggerEventPublisher : IEventPublisher
    {
        private readonly ILogger _logger;
        private readonly IEventSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerEventPublisher"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="serializer">The <see cref="IEventSerializer"/>.</param>
        public LoggerEventPublisher(ILogger<LoggerEventPublisher> logger, IEventSerializer serializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        /// <inheritdoc/>
        public async Task SendAsync(params EventData[] events) 
            => await events.ForEachAsync(async @event => _logger.LogInformation(JToken.Parse(Encoding.UTF8.GetString(await _serializer.SerializeAsync(@event).ConfigureAwait(false))).ToString(Newtonsoft.Json.Formatting.Indented))).ConfigureAwait(false);
    }
}