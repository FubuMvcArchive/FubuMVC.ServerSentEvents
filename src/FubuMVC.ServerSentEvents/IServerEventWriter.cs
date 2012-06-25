using System;

namespace FubuMVC.ServerSentEvents
{
    public interface IServerEventWriter
    {
        void WriteData(Func<object> getData, string id = null, string @event = null, int? retry = null);
        void Write(IServerEvent @event);
    }
}