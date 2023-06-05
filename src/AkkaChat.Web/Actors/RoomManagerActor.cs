// -----------------------------------------------------------------------
//  <copyright file="RoomManagerActor.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using AkkaChat.Models;

namespace AkkaChat.Web.Actors;

/// <summary>
///     Responsible for keeping track of all chat rooms that might exist inside the application
/// </summary>
public sealed class RoomManagerActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();

    public RoomManagerActor()
    {
        Receive<IWithChatRoomId>(cid =>
        {
            // CREATE IF NOT EXISTS pattern
            var chatRoomActor = Context.Child(cid.ChatRoomId).GetOrElse(() =>
            {
                return Context.ActorOf(Props.Create(() => new MessageHistoryActor(cid.ChatRoomId)),
                    cid.ChatRoomId);
            });

            //chatRoomActor.Forward(cid);
            chatRoomActor.Tell(cid, Sender);
        });
    }

    protected override void PreStart()
    {
        _log.Info("RoomManagerActor started");
    }
}