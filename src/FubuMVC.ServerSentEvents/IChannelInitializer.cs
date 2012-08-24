using System.Collections.Generic;
using System.Linq;

namespace FubuMVC.ServerSentEvents
{
    public interface IChannelInitializer<in T> where T : Topic
    {
        IEnumerable<IServerEvent> GetInitializationEvents(T Topic);
    }

    public class DefaultChannelInitializer<T> : IChannelInitializer<T> where T : Topic
    {
        public IEnumerable<IServerEvent> GetInitializationEvents(T Topic)
        {
            return Enumerable.Empty<IServerEvent>();
        }
    }
}