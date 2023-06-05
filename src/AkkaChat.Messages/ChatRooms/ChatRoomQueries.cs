// -----------------------------------------------------------------------
//  <copyright file="ChatRoomQueries.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using AkkaChat.Models;

namespace AkkaChat.Messages.ChatRooms;

/// <summary>
///     A read-only command designed to retrieve data about the current, past, or future state of an entity.
/// </summary>
public interface IChatRoomQuery : IWithChatRoomId
{
}

public static class ChatRoomQueries
{
    public record GetRecentMessages
        (string ChatRoomId, string UserId, DateTimeOffset? Until = null, int Count = 30) : IChatRoomQuery, IWithUserId;

    public record SubscribeToMessages(string ChatRoomId, IActorRef Subscriber) : IChatRoomQuery;

    public record UnsubscribeFromMessages(string ChatRoomId, IActorRef Subscriber) : IChatRoomQuery;
}