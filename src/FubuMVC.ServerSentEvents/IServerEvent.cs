namespace FubuMVC.ServerSentEvents
{
    public interface IServerEvent
    {
        string Id { get; }
        string Event { get; }
        int? Retry { get; }
        object GetData();
    }
}