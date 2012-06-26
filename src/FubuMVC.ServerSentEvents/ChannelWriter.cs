using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FubuMVC.Core.Http;

namespace FubuMVC.ServerSentEvents
{
    public class ChannelWriter<T> where T : Topic
    {
        private readonly IClientConnectivity _connectivity;
        private readonly IServerEventWriter _writer;
        private readonly ITopicChannelCache _cache;

        public ChannelWriter(IClientConnectivity connectivity, IServerEventWriter writer, ITopicChannelCache cache)
        {
            _connectivity = connectivity;
            _writer = writer;
            _cache = cache;
        }

        public void WriteMessages(T topic)
        {
            var channel = _cache.ChannelFor(topic).Channel;

            while (_connectivity.IsClientConnected() && channel.IsConnected())
            {
                var task = channel.FindEvents(topic);

                var waitHandle = new ManualResetEvent(false);

                task.ContinueWith(x =>
                {
                    if (!_connectivity.IsClientConnected())
                    {
                        waitHandle.Set();
                        return;
                    }

                    var messages = task.Result;
                    var lastSuccessfulMessage = messages
                        .TakeWhile(y => _writer.Write(y))
                        .LastOrDefault();

                    if (lastSuccessfulMessage != null)
                    {
                        topic.LastEventId = lastSuccessfulMessage.Id;
                    }

                    waitHandle.Set();
                }, TaskContinuationOptions.AttachedToParent);

                //This reduces blocking to only occur on the dedicated long running task.
                waitHandle.WaitOne(TimeSpan.FromSeconds(1), false);
            }
        }

        public Task Write(T topic)
        {
            return Task.Factory.StartNew(() => WriteMessages(topic), TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
        }
    }
}