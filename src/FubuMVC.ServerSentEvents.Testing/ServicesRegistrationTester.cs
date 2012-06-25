using FubuMVC.Core;
using FubuMVC.Core.Registration;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.ServerSentEvents.Testing
{
    [TestFixture]
    public class ServicesRegistrationTester
    {
        private ServiceGraph services;

        [SetUp]
        public void SetUp()
        {
            var registry = new FubuRegistry();
            registry.Services<ServerSentEventRegistry>();

            services = BehaviorGraph.BuildFrom(registry).Services;
        }

        [Test]
        public void EventPublisher_is_registered()
        {
            services.DefaultServiceFor<IEventPublisher>()
                .Type.ShouldEqual(typeof (EventPublisher));
        }

        [Test]
        public void default_open_type_of_IEventQueueFactory()
        {
            services.DefaultServiceFor(typeof(IEventQueueFactory<>))
                .Type.ShouldEqual(typeof(EventQueueFactory<>));
        }

        [Test]
        public void ServerEventWriter()
        {
            services.DefaultServiceFor<IServerEventWriter>()
                .Type.ShouldEqual(typeof(ServerEventWriter));
        }

        [Test]
        public void TopicChannelCache_is_registered_as_a_singleton()
        {
            services.DefaultServiceFor<ITopicChannelCache>()
                .Type.ShouldEqual(typeof(TopicChannelCache));
        }
    }
}