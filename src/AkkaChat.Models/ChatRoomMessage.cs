// -----------------------------------------------------------------------
//  <copyright file="ChatRoomMessage.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public interface IWithMessageId : IWithChatRoomId, IWithUserId
{
    string MessageId { get; }

    DateTimeOffset Timestamp { get; }
}

public record ChatRoomMessage(string MessageId, string ChatRoomId, string UserId, DateTimeOffset Timestamp,
    string Message) : IWithMessageId, IComparable<ChatRoomMessage>
{
    public int CompareTo(ChatRoomMessage? other)
    {
        return other is null ? 1 : Timestamp.CompareTo(other.Timestamp);
    }
}