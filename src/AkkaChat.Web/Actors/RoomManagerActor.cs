// -----------------------------------------------------------------------
//  <copyright file="RoomManagerActor.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using AkkaChat.Models;

namespace AkkaChat.Web.Actors;

/// <summary>
/// Responsible for keeping track of all chat rooms that might exist inside the application
/// </summary>
public sealed class RoomManagerActor : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    
    public RoomManagerActor()
    {
        Receive<IWithChatRoomId>(cid =>
        {
            // CREATE IF NOT EXISTS pattern
            IActorRef chatRoomActor = Context.Child(cid.ChatRoomId).GetOrElse(() =>
            {
                return Context.ActorOf(Props.Create(() => new MessageHistoryActor(cid.ChatRoomId)), cid.ChatRoomId);
            });
            
            chatRoomActor.Forward(cid);
        });
    }

    protected override void PreStart()
    {
        _log.Info("RoomManagerActor started");
    }
}