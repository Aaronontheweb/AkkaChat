// -----------------------------------------------------------------------
//  <copyright file="ChatRoomCommands.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using AkkaChat.Models;

namespace AkkaChat.Messages.ChatRooms;

/// <summary>
/// A request to carry out some operation that will have effects on a typically a single chatroom
/// </summary>
public interface IChatRoomCommand : IWithChatRoomId{ }

public static class ChatRoomCommands 
{
    public sealed record CreateChatRoom(string ChatRoomId, string Name, string OwnerId) : IChatRoomCommand;
    
    public sealed record JoinChatRoom(string ChatRoomId, string UserId) : IChatRoomCommand;
    
    public sealed record LeaveChatRoom(string ChatRoomId, string UserId) : IChatRoomCommand;
    
    public sealed record PostMessage(string ChatRoomId, string UserId, ChatRoomMessage Message) : IChatRoomCommand;
}