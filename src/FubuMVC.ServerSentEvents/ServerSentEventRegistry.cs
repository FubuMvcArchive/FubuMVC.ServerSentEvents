using FubuMVC.Core.Registration;

namespace FubuMVC.ServerSentEvents
{
    public class ServerSentEventRegistry : ServiceRegistry
    {
        public ServerSentEventRegistry()
        {
            SetServiceIfNone<IEventPublisher, EventPublisher>();
            SetServiceIfNone<IEventQueueFactory, EventQueueFactory>();
            SetServiceIfNone(typeof (IEventQueueFactory<>), typeof (DefaultEventQueueFactory<>));
            SetServiceIfNone<IServerEventWriter, ServerEventWriter>();
            SetServiceIfNone<ITopicChannelCache, TopicChannelCache>();
            SetServiceIfNone<IDataFormatter, DataFormatter>();
            SetServiceIfNone<IAspNetShutDownDetector, AspNetShutDownDetector>();
        }
    }
}