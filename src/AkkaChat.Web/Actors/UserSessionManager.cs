// -----------------------------------------------------------------------
//  <copyright file="UserSessionManager.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Akka.Hosting;
using AkkaChat.Models;

namespace AkkaChat.Web.Actors;

public sealed class UserSessionManager : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IRequiredActor<RoomManagerActor> _serviceProvider;

    public UserSessionManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider.GetRequiredService<IRequiredActor<RoomManagerActor>>();

        Receive<IWithUserId>(cmd =>
        {
            var userSessionActor = Context.Child(cmd.UserId).GetOrElse(() =>
            {
                var resolver = DependencyResolver.For(Context.System);
                var props = resolver.Props<UserSessionActor>(cmd.UserId);
                return Context.ActorOf(props, cmd.UserId);
            });
            userSessionActor.Forward(cmd);
        });
    }

    protected override void PreStart()
    {
        _log.Info("UserSessionManager started with reference to RoomManagerActor {0}", _serviceProvider.ActorRef);
    }
}