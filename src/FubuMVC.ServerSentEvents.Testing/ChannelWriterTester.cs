using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;
using FubuMVC.Core.Http;
using NUnit.Framework;
using Rhino.Mocks;
using FubuTestingSupport;
using System.Linq;

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
        private FakeTopic theTopic;

        [SetUp]
        public void SetUp()
        {
            theWriter = new RecordingServerEventWriter();

            var cache = MockRepository.GenerateMock<ITopicChannelCache>();
            var channel = new TopicChannel<FakeTopic>(new EventQueue<FakeTopic>());
            theChannel = channel.Channel;
            theTopic = new FakeTopic();

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
            theChannel.Write(q => q.Write(e1, e2, e3));

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e3);

            var task = theChannelWriter.Write(theTopic);

            task.Wait(150).ShouldBeTrue();

            theWriter.Events.ShouldHaveTheSameElementsAs(e1, e2, e3);
        }

        [Test]
        public void polls_for_new_events()
        {
            theChannel.Write(q => q.Write(e1, e2));

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e5);

            var task = theChannelWriter.Write(theTopic);

            task.Wait(15);
            theChannel.Write(q => q.Write(e3, e4));
            task.Wait(15);
            theChannel.Write(q => q.Write(e5));

            task.Wait(150).ShouldBeTrue();

            theWriter.Events.ShouldHaveTheSameElementsAs(e1, e2, e3, e4, e5);
        }

        [Test]
        public void does_not_poll_when_the_client_is_initially_disconnected()
        {
            theWriter.ConnectedTest = () => false;

            var task = theChannelWriter.Write(theTopic);

            task.Wait(15).ShouldBeTrue();
        }

        [Test]
        public void does_not_write_poll_results_if_the_client_has_disconnected()
        {
            var task = theChannelWriter.Write(theTopic);

            task.Wait(150).ShouldBeFalse();

            theWriter.ForceClientDisconnect();

            theChannel.Write(q => q.Write(e1));

            task.Wait(15).ShouldBeTrue();

            theWriter.Events.ShouldHaveCount(0);
        }

        [Test]
        public void does_not_poll_when_the_channel_is_initially_disconnected()
        {
            theChannel.Flush();

            var task = theChannelWriter.Write(theTopic);

            task.Wait(15).ShouldBeTrue();
        }

        [Test]
        public void does_not_poll_when_the_channel_is_disconnected_after_initial_poll()
        {
            var task = theChannelWriter.Write(theTopic);

            task.Wait(500).ShouldBeFalse();

            theChannel.Flush();

            task.Wait(15).ShouldBeTrue();
            theWriter.Events.ShouldHaveCount(0);
        }

        [Test]
        public void sets_last_event_id_to_last_successfully_written_message()
        {
            theWriter.FailOnNthWrite = 3;

            theChannel.Write(q => q.Write(e1, e2, e3));

            theWriter.ConnectedTest = () => !theWriter.Events.Contains(e2);

            var task = theChannelWriter.Write(theTopic);

            task.Wait(15).ShouldBeTrue();

            theTopic.LastEventId.ShouldEqual(e2.Id);
        }

        [Test]
        public void parent_task_is_faulted_when_writer_throws_exception()
        {
            theWriter.WriterThrows = true;

            var testTask = new Task(() =>
            {
                theChannelWriter.Write(theTopic);
                theChannel.Write(q => q.Write(e1));
            });

            testTask.RunSynchronously();

            testTask.IsFaulted.ShouldBeTrue();
        }

        [Test]
        public void does_not_produce_stack_overflow_exception()
        {
            var parentTask = new Task(() => theChannelWriter.Write(theTopic));

            parentTask.Start();

            var startTime = DateTime.Now;
            while (DateTime.Now - startTime < TimeSpan.FromSeconds(15))
            {
                theChannel.Write(q => q.Write(e1));
                Thread.Sleep(2);
            }

            theWriter.ForceClientDisconnect();
            theChannel.Flush();

            parentTask.Wait(5000).ShouldBeTrue();
        }
    }

    public class RecordingServerEventWriter : IServerEventWriter, IClientConnectivity
    {
        private readonly IList<IServerEvent> _events = new List<IServerEvent>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public int? FailOnNthWrite { get; set; }

        public bool WriterThrows { get; set; }

        public IList<IServerEvent> Events
        {
            get { return _lock.Read(() => _events.ToList()); }
        }

        public bool WriteData(object data, string id, string @event, int? retry)
        {
            throw new NotImplementedException();
        }

        public bool Write(IServerEvent @event)
        {
            if (WriterThrows)
                throw new Exception();

            if (FailOnNthWrite.HasValue && FailOnNthWrite.Value == _lock.Read(() => _events.Count + 1))
                return false;

            _lock.Write(() => _events.Add(@event));
            return true;
        }

        public Func<bool> ConnectedTest = () => true;

        public IList<Action> QueuedActions = new List<Action>();

        private long _forceDisconnect;

        public bool IsClientConnected()
        {
            return ConnectedTest() && Interlocked.Read(ref _forceDisconnect) == 0;
        }

        public void ForceClientDisconnect()
        {
            Interlocked.Increment(ref _forceDisconnect);
        }
    }
}