using NTangle.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NTangle.Test
{
    public class TestEventPublisher : EventPublisherBase
    {
        protected override async Task SendEventsAsync(EventData[] events)
        {
            var ces = new CloudEventSerializer();
            foreach (var ed in events)
            {
                Events.Add(await ces.SerializeAsync(ed).ConfigureAwait(false));
            }
        }

        public List<byte[]> Events { get; } = new List<byte[]>();
    }
}