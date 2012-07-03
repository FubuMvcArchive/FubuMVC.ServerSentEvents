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
        private IChannel<T> _channel;
        private T _topic;

        public ChannelWriter(IClientConnectivity connectivity, IServerEventWriter writer, ITopicChannelCache cache)
        {
            _connectivity = connectivity;
            _writer = writer;
            _cache = cache;
        }

        public void WriteMessages()
        {
            if (!_connectivity.IsClientConnected() || !_channel.IsConnected())
                return;

            var task = _channel.FindEvents(_topic);

            task.ContinueWith(x =>
            {
                if (!_connectivity.IsClientConnected())
                    return;

                var messages = x.Result;
                var lastSuccessfulMessage = messages
                    .TakeWhile(y => _writer.Write(y))
                    .LastOrDefault();

                if (lastSuccessfulMessage != null)
                {
                    _topic.LastEventId = lastSuccessfulMessage.Id;
                }

                WriteMessages();
            }, TaskContinuationOptions.AttachedToParent);
        }

        public Task Write(T topic)
        {
            return Task.Factory.StartNew(() =>
            {
                _topic = topic;
                _channel = _cache.ChannelFor(topic).Channel;
                WriteMessages();
            }, TaskCreationOptions.AttachedToParent);
        }
    }
}