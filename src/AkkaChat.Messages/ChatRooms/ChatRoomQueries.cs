// -----------------------------------------------------------------------
//  <copyright file="ChatRoomQueries.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using AkkaChat.Models;

namespace AkkaChat.Messages.ChatRooms;

/// <summary>
/// A read-only command designed to retrieve data about the current, past, or future state of an entity.
/// </summary>
public interface IChatRoomQuery : IWithChatRoomId
{
}

public static class ChatRoomQueries
{
    public record GetRecentMessages(string ChatRoomId, string UserId, DateTimeOffset? Until = null, int Count = 30) : IChatRoomQuery, IWithUserId;
}