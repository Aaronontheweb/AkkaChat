// -----------------------------------------------------------------------
//  <copyright file="PersistenceIdGenerators.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public static class PersistenceIdGenerators
{
    public static string IdForChatRoom(string chatRoomId) => $"chatroom-{chatRoomId}";
    
    public static string IdForUser(string userId) => $"user-{userId}";
}