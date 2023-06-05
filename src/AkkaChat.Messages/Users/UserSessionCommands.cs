// -----------------------------------------------------------------------
//  <copyright file="UserSessionCommands.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
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
    public static (CommandResultType resultType, IUserSessionEvent[] events) Process(this UserSessionState state,
        IUserSessionCommand command)
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
                return state.ActiveChatRooms.Contains(join.ChatRoomId)
                    ? (CommandResultType.NoOp, Array.Empty<IUserSessionEvent>())
                    : (CommandResultType.Success,
                        new IUserSessionEvent[] { new ChatRoomJoined(join.UserId, join.ChatRoomId) });
            case LeaveChatRoom leave:
                return state.ActiveChatRooms.Contains(leave.ChatRoomId) ?
                    (CommandResultType.Success,
                        new IUserSessionEvent[] { new ChatRoomLeft(leave.UserId, leave.ChatRoomId) }) :
                    (CommandResultType.NoOp, Array.Empty<IUserSessionEvent>());
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