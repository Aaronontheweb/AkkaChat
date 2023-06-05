// -----------------------------------------------------------------------
//  <copyright file="CommandResultType.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
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
    public static CommandResult Success(string? message = null)
    {
        return new CommandResult(CommandResultType.Success, message ?? "Ok");
    }

    public static CommandResult Failure(string message)
    {
        return new CommandResult(CommandResultType.Failure, message);
    }

    public static CommandResult NoOp(string? message = null)
    {
        return new CommandResult(CommandResultType.NoOp, message ?? "NoOp");
    }
}