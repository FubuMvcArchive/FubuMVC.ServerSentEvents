namespace FubuMVC.ServerSentEvents
{
    public interface IServerEventWriter
    {
        void WriteData(string data, string id = null, string @event = null, int? retry = null);
        void Write(ServerEvent @event);
    }
}