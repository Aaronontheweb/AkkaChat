// -----------------------------------------------------------------------
//  <copyright file="UserSessionCommands.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Actor;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Models;
using static AkkaChat.Messages.Users.UserSessionCommands;
using static AkkaChat.Messages.Users.UserSessionEvents;

namespace AkkaChat.Messages.Users;

public interface IUserSessionCommand : IWithUserId
{
}

public static class UserSessionCommands
{
    public record CreateSession(string UserId, string DisplayName) : IUserSessionCommand;

    public record JoinChatRoom(string UserId, string ChatRoomId) : IUserSessionCommand;

    public record LeaveChatRoom(string UserId, string ChatRoomId) : IUserSessionCommand;

    public record TerminateSession(string UserId) : IUserSessionCommand;
}

public static class UserStateExtensions
{
    public static async ValueTask<(CommandResultType resultType, IUserSessionEvent[] events)> ProcessAsync(this UserSessionState state,
        IUserSessionCommand command, IActorRef chatRoomActor, CancellationToken ct = default)
    {
        if (state.IsEmpty)
        {
            if (command is CreateSession session)
                return (CommandResultType.Success,
                    new IUserSessionEvent[] { new SessionCreated(session.UserId, session.DisplayName) });
            return (CommandResultType.Failure, Array.Empty<IUserSessionEvent>());
        }

        switch (command)
        {
            case JoinChatRoom join:
            {
                if(state.ActiveChatRooms.Contains(join.ChatRoomId))
                       return (CommandResultType.NoOp, Array.Empty<IUserSessionEvent>());

                try
                {
                    var joinResult = await chatRoomActor.Ask<CommandResult>(
                        new ChatRoomCommands.JoinChatRoom(join.ChatRoomId, join.UserId), ct);

                    switch (joinResult.Type)
                    {
                        case CommandResultType.Success:
                            return (joinResult.Type,
                                new IUserSessionEvent[] { new ChatRoomJoined(join.UserId, join.ChatRoomId) });
                        case CommandResultType.Failure:
                            return (joinResult.Type, Array.Empty<IUserSessionEvent>());
                        case CommandResultType.NoOp:
                            return (joinResult.Type, Array.Empty<IUserSessionEvent>());
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    return (CommandResultType.Failure, Array.Empty<IUserSessionEvent>());
                }
            }

            case LeaveChatRoom leave:
            {
                if(!state.ActiveChatRooms.Contains(leave.ChatRoomId))
                    return (CommandResultType.NoOp, Array.Empty<IUserSessionEvent>());
                
                try
                {
                    var leaveResult = await chatRoomActor.Ask<CommandResult>(
                        new ChatRoomCommands.LeaveChatRoom(leave.ChatRoomId, leave.UserId), ct);

                    switch (leaveResult.Type)
                    {
                        case CommandResultType.Success:
                            return (leaveResult.Type,
                                new IUserSessionEvent[] { new ChatRoomLeft(leave.UserId, leave.ChatRoomId) });
                        case CommandResultType.Failure:
                            return (leaveResult.Type, Array.Empty<IUserSessionEvent>());
                        case CommandResultType.NoOp:
                            return (leaveResult.Type, Array.Empty<IUserSessionEvent>());
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    return (CommandResultType.Failure, Array.Empty<IUserSessionEvent>());
                }
            }
               
            case TerminateSession terminate:
                return (CommandResultType.Success,
                    new IUserSessionEvent[] { new SessionTerminated(terminate.UserId) });
            case CreateSession _:
                return (CommandResultType.NoOp, Array.Empty<IUserSessionEvent>());
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    public static UserSessionState Apply(this UserSessionState state, IUserSessionEvent @event)
    {
        switch (@event)
        {
            case SessionCreated created:
                return new UserSessionState(created.UserId, created.DisplayName, ImmutableHashSet<string>.Empty);
            case ChatRoomJoined joined:
                return state with { ActiveChatRooms = state.ActiveChatRooms.Add(joined.ChatRoomId) };
            case ChatRoomLeft left:
                return state with { ActiveChatRooms = state.ActiveChatRooms.Remove(left.ChatRoomId) };
            case SessionTerminated _:
                return UserSessionState.Empty;
            default:
                throw new ArgumentOutOfRangeException(nameof(@event));
        }
    }
}