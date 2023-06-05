﻿// -----------------------------------------------------------------------
//  <copyright file="UserSessionState.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public interface IWithUserId
{
    string UserId { get; }
}

public record UserSessionState(string UserId, string DisplayName) : IWithUserId
{
    public static readonly UserSessionState Empty = new(string.Empty, string.Empty);
}