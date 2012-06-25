using FubuMVC.Core.Registration;

namespace FubuMVC.ServerSentEvents
{
    public class ServerSentEventRegistry : ServiceRegistry
    {
        public ServerSentEventRegistry()
        {
            SetServiceIfNone<IEventPublisher, EventPublisher>();
            SetServiceIfNone(typeof (IEventQueueFactory<>), typeof (EventQueueFactory<>));
            SetServiceIfNone<IServerEventWriter, ServerEventWriter>();
            SetServiceIfNone<ITopicChannelCache, TopicChannelCache>();
        }
    }
}