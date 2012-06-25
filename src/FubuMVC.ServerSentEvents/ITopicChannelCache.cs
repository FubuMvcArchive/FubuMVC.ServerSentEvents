using System;
using System.Collections.Generic;

namespace FubuMVC.ServerSentEvents
{
    public interface ITopicChannelCache
    {
        ITopicChannel<T> ChannelFor<T>(T topic) where T : Topic;
        void ClearAll();

        void SpinUpTopics<T>(Func<IEnumerable<T>> topics) where T : Topic;
    }
}