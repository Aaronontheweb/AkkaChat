// -----------------------------------------------------------------------
//  <copyright file="CommandResultType.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace AkkaChat.Models;

public enum CommandResultType
{
    Success,
    Failure,
    NoOp
}

public record CommandResult(CommandResultType Type, string Message)
{
    public static CommandResult Success(string? message = null) => new(CommandResultType.Success, message ?? "Ok");
    public static CommandResult Failure(string message) => new(CommandResultType.Failure, message);
    public static CommandResult NoOp(string? message = null) => new(CommandResultType.NoOp, message ?? "NoOp");
}