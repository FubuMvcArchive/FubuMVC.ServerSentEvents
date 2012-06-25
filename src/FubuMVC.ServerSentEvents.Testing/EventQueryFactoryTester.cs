using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.ServerSentEvents.Testing
{
    [TestFixture]
    public class EventQueryFactoryTester
    {
        [Test]
        public void build_for_returns_a_simple_queue()
        {
            var factory = new EventQueueFactory<FakeTopic>();
            factory.BuildFor(new FakeTopic()).ShouldBeOfType<EventQueue<FakeTopic>>();
        }
    }
}