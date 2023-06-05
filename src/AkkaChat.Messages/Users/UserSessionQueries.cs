// -----------------------------------------------------------------------
//  <copyright file="UserSessionQueries.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using AkkaChat.Models;

namespace AkkaChat.Messages.Users;

public interface IUserSessionQuery : IWithUserId
{
}

public static class UserSessionQueries
{
    public record GetSessionState(string UserId) : IUserSessionQuery;
}