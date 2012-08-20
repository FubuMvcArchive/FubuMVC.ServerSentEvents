using FubuMVC.Core;
using FubuMVC.Core.Behaviors;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using NUnit.Framework;
using FubuTestingSupport;
using System.Linq;
using StructureMap;
using FubuMVC.StructureMap;

namespace FubuMVC.ServerSentEvents.Testing
{

    [TestFixture]
    public class Can_build_from_container
    {
        [Test]
        public void end_to_end()
        {
            var registry = new FubuRegistry();
            registry.Applies.ToThisAssembly();
            registry.Import<ServerSentEventsExtension>();

            var container = new Container();
            FubuApplication.For(registry).StructureMap(container).Bootstrap();
            var graph = container.GetInstance<BehaviorGraph>();
            var chain = graph.BehaviorFor<ChannelWriter<FakeTopic>>(x => x.Write(null));

            

            container.GetInstance<IActionBehavior>(chain.UniqueId.ToString());
        }
    }

    [TestFixture]
    public class ServerSentEventExtensionIntegratedTester
    {
        private BehaviorGraph theGraph;

        [SetUp]
        public void SetUp()
        {
            var registry = new FubuRegistry();
            registry.Applies.ToThisAssembly();
            registry.Import<ServerSentEventsExtension>();

            theGraph = BehaviorGraph.BuildFrom(registry);
        }

        [Test]
        public void should_have_an_endpoint_for_concrete_topic_class()
        {
            theGraph.BehaviorFor<ChannelWriter<FakeTopic>>(x => x.Write(null))
                .ShouldNotBeNull();

            theGraph.BehaviorFor<ChannelWriter<DifferentTopic>>(x => x.Write(null))
                .ShouldNotBeNull();
        }

        [Test]
        public void should_have_url_patter_for_the_topic_classes()
        {
            theGraph.BehaviorFor<ChannelWriter<FakeTopic>>(x => x.Write(null))
                .Route.Pattern.ShouldEqual("_events/fake");
        }

        [Test]
        public void should_have_url_patter_for_the_topic_classes_with_route()
        {
            var route = theGraph
                .BehaviorFor<ChannelWriter<DifferentTopic>>(x => x.Write(null))
                .Route;

            route.Pattern.ShouldEqual("_events/different/{Name}");
        }
    }
}