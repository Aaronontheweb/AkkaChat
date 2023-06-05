// -----------------------------------------------------------------------
//  <copyright file="AkkaChatActorConfig.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Hosting;
using AkkaChat.Web.Actors;

namespace AkkaChat.Web.Config;

public static class AkkaChatActorConfig
{
    public static AkkaConfigurationBuilder AddAkkaChatActors(this AkkaConfigurationBuilder builder)
    {
        return builder.WithActors((system, registry, _) =>
            {
                IActorRef roomManager = system.ActorOf(Props.Create(() => new RoomManagerActor()), "rooms");
                registry.Register<RoomManagerActor>(roomManager);
            })
            .WithActors((system, registry, resolver) =>
            {
                // populate constructor arguments for actor using the IServiceProvider (DI)
                Props userSessionManagerProps = resolver.Props<UserSessionManager>();
                IActorRef userSessionManager = system.ActorOf(userSessionManagerProps, "user-session-manager");
                registry.Register<UserSessionManager>(userSessionManager);
            });
    }
}