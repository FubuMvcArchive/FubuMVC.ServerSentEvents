using System;
using System.Collections.Generic;
using Bottles;
using FubuCore.Reflection;
using FubuMVC.Core;
using FubuCore;
using System.Linq;
using FubuMVC.Core.Registration;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;
using FubuMVC.Core.Runtime;

namespace FubuMVC.ServerSentEvents
{
    public class ServerSentEventsExtension : IFubuRegistryExtension
    {
        public void Configure(FubuRegistry registry)
        {
            registry.Services<ServerSentEventRegistry>();

            registry.Actions.FindWith<TopicActions>();
            registry.Routes.UrlPolicy<TopicUrls>();
        }
    }

    public class TopicUrls : IUrlPolicy
    {
        public bool Matches(ActionCall call, IConfigurationObserver log)
        {
            return call.HandlerType.Closes(typeof (ChannelWriter<>));
        }

        public IRouteDefinition Build(ActionCall call)
        {
            var topicType = call.HandlerType.GetGenericArguments().Single();
            var route = RouteBuilder.Build(topicType, "_events/" + topicType.Name.ToLower().Replace("topic", ""));
        
        
            new RouteDefinitionResolver().InputPolicy.AlterRoute(route, call);

            return route;
        }
    }

    public class TopicActions : IActionSource
    {
        public IEnumerable<ActionCall> FindActions(TypePool types)
        {
            var methodName = ReflectionHelper.GetMethod<ChannelWriter<Topic>>(x => x.Write(null))
                .Name;

            var topicTypes = types.TypesMatching(x => x.IsConcreteTypeOf<Topic>());

            foreach (var topicType in topicTypes)
            {
                var handlerType = typeof (ChannelWriter<>).MakeGenericType(topicType);
                var method = handlerType.GetMethod(methodName);

                yield return new ActionCall(handlerType, method);
            }
        }
    }
}