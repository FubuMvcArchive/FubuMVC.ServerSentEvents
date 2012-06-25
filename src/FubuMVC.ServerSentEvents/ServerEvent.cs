namespace FubuMVC.ServerSentEvents
{
    public class ServerEvent : IServerEvent
    {
        public ServerEvent(string id, string data)
        {
            Id = id;
            Data = data;
        }

        public string Data { get; private set; }
        public string Id { get; private set; }
        public string Event { get; set; }
        public int? Retry { get; set; }

        public object GetData()
        {
            return Data;
        }
    }
}