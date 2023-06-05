// -----------------------------------------------------------------------
//  <copyright file="ChatRoomEvents.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using AkkaChat.Models;
using static AkkaChat.Messages.ChatRooms.ChatRoomCommands;
using static AkkaChat.Messages.ChatRooms.ChatRoomEvents;

namespace AkkaChat.Messages.ChatRooms;

/// <summary>
///     An event is a matter of fact - something that definitely happened inside the system.
/// </summary>
public interface IChatRoomEvent : IWithChatRoomId
{
}

public static class ChatRoomEvents
{
    public record ChatRoomCreated(string ChatRoomId, string Name, string OwnerId) : IChatRoomEvent;

    public record ChatRoomJoined(string ChatRoomId, string UserId) : IChatRoomEvent;

    public record ChatRoomLeft(string ChatRoomId, string UserId) : IChatRoomEvent;

    public record ChatRoomMessagePosted(ChatRoomMessage Message) : IChatRoomEvent
    {
        public string UserId => Message.UserId;
        public string ChatRoomId => Message.ChatRoomId;
    }
}

public static class ChatRoomStateExtensions
{
    public const int MaxRecentMessages = 30;

    public static (CommandResultType resultType, IChatRoomEvent[] events) Process(this ChatRoomState state,
        IChatRoomCommand command)
    {
        if (state.IsEmpty)
            if (command is CreateChatRoom createChatRoom)
                return (CommandResultType.Success,
                    new IChatRoomEvent[]
                    {
                        new ChatRoomCreated(createChatRoom.ChatRoomId, createChatRoom.Name, createChatRoom.OwnerId)
                    });
            else
                return (CommandResultType.Failure,
                    Array.Empty<IChatRoomEvent>()); // can't process - chatroom is not created yet


        switch (command)
        {
            case PostMessage postMessage:
                // TODO: add more robust message validation here
                return state.ActiveUsers.Contains(postMessage.UserId)
                    ? (CommandResultType.Success,
                        new IChatRoomEvent[] { new ChatRoomMessagePosted(postMessage.Message) })
                    : (CommandResultType.Failure,
                        Array.Empty<IChatRoomEvent>()); // can't process - user is not in the chatroom
            case CreateChatRoom _:
                return (CommandResultType.NoOp, Array.Empty<IChatRoomEvent>()); // duplicate - chatroom already exists
            case JoinChatRoom join:
                return state.ActiveUsers.Contains(join.UserId)
                    ? (CommandResultType.NoOp, Array.Empty<IChatRoomEvent>()) // duplicate - user already joined
                    : (CommandResultType.Success,
                        new IChatRoomEvent[] { new ChatRoomJoined(state.ChatRoomId, join.UserId) });
            case LeaveChatRoom leave:
                return state.ActiveUsers.Contains(leave.UserId)
                    ? (CommandResultType.Success,
                        new IChatRoomEvent[] { new ChatRoomLeft(state.ChatRoomId, leave.UserId) })
                    : (CommandResultType.NoOp,
                        Array.Empty<IChatRoomEvent>()); // duplicate - user already left (or never joined)
            default:
                throw new ArgumentOutOfRangeException(nameof(command));
        }
    }

    public static ChatRoomState Apply(this ChatRoomState state, IChatRoomEvent @event)
    {
        switch (@event)
        {
            case ChatRoomMessagePosted posted:
            {
                return state with
                {
                    RecentMessages = state.RecentMessages.Add(posted.Message).Take(MaxRecentMessages)
                        .ToImmutableSortedSet(),
                    TotalMessages = state.TotalMessages + 1
                };
            }
            case ChatRoomJoined joined:
            {
                return state with
                {
                    ActiveUsers = state.ActiveUsers.Add(joined.UserId)
                };
            }
            case ChatRoomLeft left:
            {
                return state with
                {
                    ActiveUsers = state.ActiveUsers.Remove(left.UserId)
                };
            }
            case ChatRoomCreated created:
            {
                return state with
                {
                    ChatRoomId = created.ChatRoomId,
                    Name = created.Name,
                    OwnerId = created.OwnerId
                };
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(@event));
        }
    }
}