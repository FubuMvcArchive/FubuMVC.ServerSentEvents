using System.Linq;
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

                // TODO -- Needs to deal w/ timeouts
                while (!task.Wait(1000))
                {
                    if (!_connectivity.IsClientConnected()) return;
                }

                var messages = task.Result;
                var lastSuccessfulMessage = messages
                    .TakeWhile(x => _writer.Write(x))
                    .LastOrDefault();

                if (lastSuccessfulMessage != null)
                {
                    topic.LastEventId = lastSuccessfulMessage.Id;
                }
            }
        }

        public Task Write(T topic)
        {
            return Task.Factory.StartNew(() => WriteMessages(topic), TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
        }
    }
}