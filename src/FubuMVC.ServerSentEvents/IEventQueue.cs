using System.Collections.Generic;

namespace FubuMVC.ServerSentEvents
{
    public interface IEventQueue<T> where T : Topic
    {
        IEnumerable<ServerEvent> FindQueuedEvents(T topic);
        void Write(params ServerEvent[] events);
    }
}