using System.Collections.Generic;
using System.Linq;
using FubuCore;

namespace FubuMVC.ServerSentEvents
{
    public class ServerEventList<T> where T : ServerEvent
    {
        private readonly IList<T> _events = new List<T>();

        public IList<T> AllEvents
        {
            get { return _events; }
        }

        public void Add(IEnumerable<T> events)
        {
            _events.AddRange(events);
        }

        public void Add(T @event)
        {
            _events.Add(@event);
        }


        public IEnumerable<ServerEvent> FindQueuedEvents(Topic topic)
        {
            if (topic == null || topic.LastEventId.IsEmpty())
            {
                return _events.ToList();
            }



            if (!_events.Any() || _events.Last().Id == topic.LastEventId) return Enumerable.Empty<ServerEvent>();

            var lastEvent = _events.FirstOrDefault(x => x.Id == topic.LastEventId);
            if (lastEvent == null)
            {
                return _events.ToList();
            }

            var index = _events.IndexOf(lastEvent);
            return _events.Skip(index + 1).ToList();
        }
    }
}