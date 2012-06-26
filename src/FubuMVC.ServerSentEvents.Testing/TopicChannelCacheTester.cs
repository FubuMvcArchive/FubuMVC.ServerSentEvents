using FubuMVC.StructureMap;
using FubuTestingSupport;
using NUnit.Framework;
using StructureMap;

namespace FubuMVC.ServerSentEvents.Testing
{
    [TestFixture]
    public class TopicChannelCacheTester
    {
        private TopicChannelCache theCache;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new Container(x => x.For(typeof(IEventQueueFactory<>)).Use(typeof(DefaultEventQueueFactory<>)));

            var services = new StructureMapServiceLocator(_container);

            theCache = new TopicChannelCache(services);
        }

        [Test]
        public void caches_against_a_single_topic_family()
        {
            var topic1 = new FakeTopic
            {
                Name = "Tom"
            };

            var topic2 = new FakeTopic
            {
                Name = "Todd"
            };

            theCache.ChannelFor(topic1).ShouldNotBeNull();
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));

            theCache.ChannelFor(topic2).ShouldBeTheSameAs(theCache.ChannelFor(topic2));


            theCache.ChannelFor(topic1).ShouldNotBeTheSameAs(theCache.ChannelFor(topic2));
        }

        [Test]
        public void caches_against_a_multiple_topic_families()
        {
            // All relatives of mine from the same family.  In-law finally
            // rebelled and named a child "Parker"
            var topic1 = new FakeTopic
            {
                Name = "Tom"
            };

            var topic2 = new FakeTopic
            {
                Name = "Todd"
            };

            var topic3 = new DifferentTopic{
                Name = "Trevor"
            };

            var topic4 = new DifferentTopic
            {
                Name = "Trent"
            };

            theCache.ChannelFor(topic1).ShouldNotBeNull();
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));
            theCache.ChannelFor(topic1).ShouldBeTheSameAs(theCache.ChannelFor(topic1));

            theCache.ChannelFor(topic2).ShouldBeTheSameAs(theCache.ChannelFor(topic2));


            theCache.ChannelFor(topic1).ShouldNotBeTheSameAs(theCache.ChannelFor(topic2));

            theCache.ChannelFor(topic3).ShouldBeTheSameAs(theCache.ChannelFor(topic3));

            theCache.ChannelFor(topic4).ShouldBeTheSameAs(theCache.ChannelFor(topic4));

            theCache.ChannelFor(topic3).ShouldNotBeTheSameAs(theCache.ChannelFor(topic4));
        }
    }
}