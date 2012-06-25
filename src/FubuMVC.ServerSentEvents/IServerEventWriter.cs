using System;

namespace FubuMVC.ServerSentEvents
{
    public interface IServerEventWriter
    {
        bool WriteData(Func<object> getData, string id = null, string @event = null, int? retry = null);
        bool Write(IServerEvent @event);
    }
}