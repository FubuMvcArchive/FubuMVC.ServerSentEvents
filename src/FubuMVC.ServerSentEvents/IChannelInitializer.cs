using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FubuMVC.ServerSentEvents
{
    public interface IChannelInitializer<in T> where T : Topic
    {
        Task<IEnumerable<IServerEvent>> GetInitializationEvents(T Topic);
    }

    public class DefaultChannelInitializer<T> : IChannelInitializer<T> where T : Topic
    {
        public Task<IEnumerable<IServerEvent>> GetInitializationEvents(T Topic)
        {
            var result = new TaskCompletionSource<IEnumerable<IServerEvent>>();
            result.SetResult(Enumerable.Empty<IServerEvent>());
            return result.Task;
        }
    }
}