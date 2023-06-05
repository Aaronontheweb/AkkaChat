// -----------------------------------------------------------------------
//  <copyright file="ChatRoomState.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;

namespace AkkaChat.Models;

/// <summary>
///     Interface for all entities and messages that belong to the "chatroom" domain
/// </summary>
public interface IWithChatRoomId
{
    string ChatRoomId { get; }
}

public record ChatRoomState(string ChatRoomId, string Name, string OwnerId, ImmutableHashSet<string> ActiveUsers,
    ImmutableSortedSet<ChatRoomMessage> RecentMessages, int TotalMessages = 0) : IWithChatRoomId
{
    public static ChatRoomState Empty => new(string.Empty, string.Empty, string.Empty, ImmutableHashSet<string>.Empty,
        ImmutableSortedSet<ChatRoomMessage>.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(OwnerId);
}