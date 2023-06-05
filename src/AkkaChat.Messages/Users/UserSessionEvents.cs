// -----------------------------------------------------------------------
//  <copyright file="UserSessionEvents.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
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