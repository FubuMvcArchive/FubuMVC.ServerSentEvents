using System;
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
            SetServiceIfNone(typeof (IChannelInitializer<>), typeof (DefaultChannelInitializer<>));
            SetServiceIfNone<IServerEventWriter, ServerEventWriter>();
            SetServiceIfNone<ITopicChannelCache, TopicChannelCache>();
            SetServiceIfNone<IDataFormatter, DataFormatter>();
            if (Type.GetType("Mono.Runtime") == null)
            {
                SetServiceIfNone<IAspNetShutDownDetector, AspNetShutDownDetector>();
            }
            else
            {
                SetServiceIfNone<IAspNetShutDownDetector, NulloShutdownDetector>();
            }
        }
    }
}