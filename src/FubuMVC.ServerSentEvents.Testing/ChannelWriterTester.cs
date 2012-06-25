using System;
using System.Collections.Generic;
using FubuMVC.Core.Http;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;

namespace FubuMVC.ServerSentEvents.Testing
{
    [TestFixture]
    public class ChannelWriterTester
    {
        private IChannel<FakeTopic> theChannel;
        private ChannelWriter<FakeTopic> theChannelWriter;
        private RecordingServerEventWriter theWriter;
        private IServerEvent e1;
        private IServerEvent e2;
        private IServerEvent e3;
        private IServerEvent e4;
        private IServerEvent e5;
        private FakeTopic theTopic = new FakeTopic();

        [SetUp]
        public void SetUp()
        {
            theWriter = new RecordingServerEventWriter();

            var cache = MockRepository.GenerateMock<ITopicChannelCache>();
            var channel = new TopicChannel<FakeTopic>(new EventQueue<FakeTopic>());
            theChannel = channel.Channel;

            cache.Stub(x => x.ChannelFor(theTopic)).Return(channel);

            theChannelWriter = new ChannelWriter<FakeTopic>(theWriter, theWriter, cache);

            e1 = new ServerEvent("1", "data-1");
            e2 = new ServerEvent("2", "data-2");
            e3 = new ServerEvent("3", "data-3");
            e4 = new ServerEvent("4", "data-4");
            e5 = new ServerEvent("5", "data-5");
        }

        [Test]
        public void simple_scenario_when_there_are_already_events_queued_up()
        {
            theChannel.Write(q =>
            {
                q.Write(e1, e2, e3);
            });

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e3);

            var task = theChannelWriter.Write(theTopic);

            task.Wait();

            theWriter.Events.ShouldHaveTheSameElementsAs(e1, e2, e3);
        }

        [Test]
        public void polls_for_new_events()
        {
            theChannel.Write(q =>
            {
                q.Write(e1, e2);
            });

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e5);

            var task = theChannelWriter.Write(theTopic);

            theChannel.Write(q => q.Write(e3, e4));
            theChannel.Write(q => q.Write(e5));

            task.Wait();

            theWriter.Events.ShouldHaveTheSameElementsAs(e1, e2, e3, e4, e5);
        }

        [Test]
        public void stops_polling_when_the_client_disconnects()
        {
            theWriter.ConnectedTest = () => false;

            theChannelWriter.Write(theTopic).Wait();

            // If you finish, you've succeeded
        }

        [Test]
        public void sets_last_event_id_to_last_successfully_written_message()
        {
            theWriter.FailOnNthWrite = 3;

            theChannel.Write(q =>
            {
                q.Write(e1, e2, e3);
            });

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e2);

            var task = theChannelWriter.Write(theTopic);

            task.Wait();

            theTopic.LastEventId.ShouldEqual(e2.Id);
        }

    }

    public class RecordingServerEventWriter : IServerEventWriter, IClientConnectivity
    {
        public readonly IList<IServerEvent> Events = new List<IServerEvent>();

        public int? FailOnNthWrite { get; set; }
        private int _writeCount;

        public bool WriteData(Func<object> getData, string id, string @event, int? retry)
        {
            throw new NotImplementedException();
        }

        public bool Write(IServerEvent @event)
        {
            _writeCount++;

            if (FailOnNthWrite.HasValue && FailOnNthWrite.Value == _writeCount)
                return false;

            Events.Add(@event);
            return true;
        }

        public Func<bool> ConnectedTest = () => true;

        public IList<Action> QueuedActions = new List<Action>();

        public bool IsClientConnected()
        {
            return ConnectedTest();
        }
    }
}