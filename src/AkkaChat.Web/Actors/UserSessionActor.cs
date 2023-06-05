// -----------------------------------------------------------------------
//  <copyright file="UserSessionActor.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Akka.Hosting;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Messages.Users;
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
            IActorRef userSessionActor = Context.Child(cmd.UserId).GetOrElse(() =>
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

public sealed class UserSessionActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    public UserSessionState State { get; private set; } = UserSessionState.Empty;
    private readonly IActorRef _chatRoomActors;

    public UserSessionActor(string userId, IRequiredActor<RoomManagerActor> chatRoomActors)
    {
        _chatRoomActors = chatRoomActors.ActorRef;
        State = State with { UserId = userId };

        Receive<IUserSessionCommand>(cmd =>
        {
            var (resultType, events) = State.Process(cmd);

            switch (resultType)
            {
                case CommandResultType.Failure:
                    Sender.Tell(CommandResult.Failure($"Failed to process command {cmd}"));
                    break;
                case CommandResultType.NoOp:
                    Sender.Tell(CommandResult.NoOp());
                    break;
            }

            foreach (var @event in events)
            {
                _log.Info("UserSessionActor: Processing {0}", @event);
                
                switch (@event)
                {
                    case UserSessionEvents.ChatRoomJoined joined:
                        _chatRoomActors.Tell(new ChatRoomCommands.JoinChatRoom(joined.ChatRoomId, userId));
                        break;
                    case UserSessionEvents.ChatRoomLeft joinedLeft:
                        _chatRoomActors.Tell(new ChatRoomCommands.LeaveChatRoom(joinedLeft.ChatRoomId, userId));
                        break;
                }
                
                State = State.Apply(@event);
            }

            Sender.Tell(CommandResult.Success());
        });
    }
}