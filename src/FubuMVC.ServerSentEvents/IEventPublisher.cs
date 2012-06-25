using System;

namespace FubuMVC.ServerSentEvents
{
    public interface IEventPublisher
    {
        void WriteTo<T>(T topic, params ServerEvent[] events) where T : Topic;

        void WriteTo<T, TQueue>(T topic, Action<TQueue> write)
            where T : Topic
            where TQueue : class;
    }
}