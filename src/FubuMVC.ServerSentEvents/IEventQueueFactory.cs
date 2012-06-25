namespace FubuMVC.ServerSentEvents
{
    public interface IEventQueueFactory<T> where T : Topic
    {
        IEventQueue<T> BuildFor(T topic);
    }
}