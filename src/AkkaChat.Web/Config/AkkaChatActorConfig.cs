// -----------------------------------------------------------------------
//  <copyright file="AkkaChatActorConfig.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
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
                var roomManager = system.ActorOf(Props.Create(() => new RoomManagerActor()), "rooms");
                registry.Register<RoomManagerActor>(roomManager);
            })
            .WithActors((system, registry, resolver) =>
            {
                // populate constructor arguments for actor using the IServiceProvider (DI)
                var userSessionManagerProps = resolver.Props<UserSessionManager>();
                var userSessionManager = system.ActorOf(userSessionManagerProps, "user-session-manager");
                registry.Register<UserSessionManager>(userSessionManager);
            });
    }
}