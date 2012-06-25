using FubuMVC.Core.Runtime;

namespace FubuMVC.ServerSentEvents
{
    public class Topic
    {
        [HeaderValue("Last-Event-ID")]
        public string LastEventId { get; set; }
    }
}