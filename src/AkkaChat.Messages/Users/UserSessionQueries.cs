﻿// -----------------------------------------------------------------------
//  <copyright file="UserSessionQueries.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using AkkaChat.Models;

namespace AkkaChat.Messages.Users;

public interface IUserSessionQuery : IWithUserId{}

public static class UserSessionQueries
{
    public record GetSessionState(string UserId) : IUserSessionQuery;
}