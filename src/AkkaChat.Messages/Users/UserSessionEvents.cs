// -----------------------------------------------------------------------
//  <copyright file="UserSessionEvents.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using AkkaChat.Models;

namespace AkkaChat.Messages.Users;

public interface IUserSessionEvent : IWithUserId
{
}

public static class UserSessionEvents
{
    public record SessionCreated(string UserId, string DisplayName) : IUserSessionEvent;

    public record ChatRoomJoined(string UserId, string ChatRoomId) : IUserSessionEvent;

    public record ChatRoomLeft(string UserId, string ChatRoomId) : IUserSessionEvent;

    public record SessionTerminated(string UserId) : IUserSessionEvent;
}