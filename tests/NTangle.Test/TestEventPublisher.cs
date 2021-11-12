using NTangle.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTangle.Test
{
    /// <summary>
    /// Simulates an event send by first serializing to a <i>CloudEvent</i> and then adding to the <see cref="Events"/> list.
    /// </summary>
    public class TestEventPublisher : IEventPublisher
    {
        /// <summary>
        /// Gets the list of sent <i>CloudEvents</i>.
        /// </summary>
        public List<byte[]> Events { get; } = new List<byte[]>();

        /// <inheritdoc/>
        public async Task SendAsync(params EventData[] events)
        {
            var ces = new CloudEventSerializer();
            foreach (var ed in events)
            {
                Events.Add(await ces.SerializeAsync(ed).ConfigureAwait(false));
            }
        }
    }
}