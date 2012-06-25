using System;
using System.Collections.Generic;

namespace FubuMVC.ServerSentEvents
{
    public class EventQueue<T> : IEventQueue<T> where T : Topic
    {
        private readonly ServerEventList<ServerEvent> _events = new ServerEventList<ServerEvent>();

        public void Clear()
        {
            _events.AllEvents.Clear();
        }

        public IList<ServerEvent> AllEvents
        {
            get { return _events.AllEvents; }
        }

        public IEnumerable<ServerEvent> FindQueuedEvents(T topic)
        {
            return _events.FindQueuedEvents(topic);
        }

        public void Write(params ServerEvent[] events)
        {
            _events.Add(events);
        }
    }
}