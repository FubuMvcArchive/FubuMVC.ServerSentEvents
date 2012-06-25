namespace FubuMVC.ServerSentEvents
{
    public class EventQueueFactory<T> : IEventQueueFactory<T> where T : Topic
    {
        public IEventQueue<T> BuildFor(T topic)
        {
            return new EventQueue<T>();
        }
    }
}