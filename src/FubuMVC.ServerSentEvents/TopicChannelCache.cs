using System;
using System.Collections.Generic;
using System.Threading;
using FubuCore;
using FubuCore.Util;

namespace FubuMVC.ServerSentEvents
{
    public class TopicChannelCache : ITopicChannelCache
    {
        private readonly Cache<Type, ITopicFamily> _families;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public TopicChannelCache(IServiceLocator services)
        {
            _families = new Cache<Type, ITopicFamily>(type =>
            {
                var familyType = typeof (TopicFamily<>).MakeGenericType(type);
                return (ITopicFamily) services.GetInstance(familyType);
            });
        }

        public ITopicChannel<T> ChannelFor<T>(T topic) where T : Topic
        {
            return _lock.Read(() =>
            {
                return _families[typeof (T)].As<TopicFamily<T>>().ChannelFor(topic);
            });
        }

        public void ClearAll()
        {
            _lock.Write(() =>
            {
                _families.Each(x => x.Flush());

                _families.ClearAll();
            });
        }

        public void SpinUpTopics<T>(Func<IEnumerable<T>> topics) where T : Topic
        {
            _lock.Write(() =>
            {
                var family = _families[typeof (T)].As<TopicFamily<T>>();
                topics().Each(family.SpinUpChannel);
            });
        }
    }
}