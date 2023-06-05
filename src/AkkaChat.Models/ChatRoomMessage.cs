// -----------------------------------------------------------------------
//  <copyright file="ChatRoomMessage.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public interface IWithMessageId : IWithChatRoomId, IWithUserId
{
    string MessageId { get; }
    
    DateTimeOffset Timestamp { get; }
}

public record ChatRoomMessage(string MessageId, string ChatRoomId, string UserId, DateTimeOffset Timestamp, string Message) : IWithMessageId, IComparable<ChatRoomMessage>
{
    public int CompareTo(ChatRoomMessage? other)
    {
        return other is null ? 1 : Timestamp.CompareTo(other.Timestamp);
    }
}