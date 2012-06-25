using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuCore;

namespace FubuMVC.ServerSentEvents
{
    public class Channel<TTopic> : IChannel<TTopic> where TTopic : Topic
    {
        private readonly IEventQueue<TTopic> _queue;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        
        private readonly IList<QueuedRequest> _outstandingRequests = new List<QueuedRequest>();
        private bool _isConnected;

        public Channel(IEventQueue<TTopic> queue)
        {
            _queue = queue;
            _isConnected = true;
        }

        public void Flush()
        {
            _lock.Write(() =>
            {
                _outstandingRequests.Each(x => x.Source.SetResult(Enumerable.Empty<IServerEvent>()));

                _isConnected = false;
            });
        }

        public Task<IEnumerable<IServerEvent>> FindEvents(TTopic topic)
        {
            var events = _lock.Read(() => _queue.FindQueuedEvents(topic));

            var source = new TaskCompletionSource<IEnumerable<IServerEvent>>();
            if (events.Any())
            {
                source.SetResult(events);
            }
            else
            {
                _lock.Write(() => _outstandingRequests.Add(new QueuedRequest(source, topic)));
            }

            return source.Task;
        }

        public void Write(Action<IEventQueue<TTopic>> action)
        {
            _lock.Write(() =>
            {
                action(_queue);
                publishToListeners();
            });
        }

        public bool IsConnected()
        {
            return _isConnected;
        }

        private void publishToListeners()
        {
            _outstandingRequests.Each(x =>
            {
                var events = _queue.FindQueuedEvents(x.Topic);
                x.Source.SetResult(events);
            });

            _outstandingRequests.Clear();
        }

        public class QueuedRequest
        {
            public QueuedRequest(TaskCompletionSource<IEnumerable<IServerEvent>> source, TTopic topic)
            {
                Source = source;
                Topic = topic;
            }

            public TaskCompletionSource<IEnumerable<IServerEvent>> Source { get; private set; }
            public TTopic Topic { get; private set; }
        }


    }
}