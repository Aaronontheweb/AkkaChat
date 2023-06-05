// -----------------------------------------------------------------------
//  <copyright file="PersistenceIdGenerators.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public static class PersistenceIdGenerators
{
    public static string IdForChatRoom(string chatRoomId)
    {
        return $"chatroom-{chatRoomId}";
    }

    public static string IdForUser(string userId)
    {
        return $"user-{userId}";
    }
}