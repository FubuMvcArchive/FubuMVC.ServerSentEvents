using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FubuMVC.ServerSentEvents
{
    public interface IChannel
    {
        void Flush();
    }

    public interface IChannel<T> : IChannel where T : Topic
    {
        Task<IEnumerable<ServerEvent>> FindEvents(T topic);
        void Write(Action<IEventQueue<T>> action);
        bool IsConnected();
    }
}