using System.Collections.Generic;

namespace FubuMVC.ServerSentEvents
{
    public interface IEventQueue<in T> where T : Topic
    {
        IEnumerable<IServerEvent> FindQueuedEvents(T topic);
        void Write(params IServerEvent[] events);
    }
}