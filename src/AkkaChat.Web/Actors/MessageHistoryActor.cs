// -----------------------------------------------------------------------
//  <copyright file="MessageHistoryActor.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Event;
using Akka.Actor;
using Akka.Persistence;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Models;
using static AkkaChat.Models.PersistenceIdGenerators;

namespace AkkaChat.Web.Actors;

public sealed class MessageHistoryActor : ReceivePersistentActor
{
    public ChatRoomState State { get; private set; } = ChatRoomState.Empty;
    private readonly ILoggingAdapter _log = Context.GetLogger();
    public override string PersistenceId { get; }

    public MessageHistoryActor(string chatroomId)
    {
        PersistenceId = IdForChatRoom(chatroomId);
        
        Recovers();
        Commands();
    }

    private void Recovers()
    {
        Recover<IChatRoomEvent>(@event =>
        {
            State = State.Apply(@event);
        });

        Recover<SnapshotOffer>(offer =>
        {
            if(offer.Snapshot is ChatRoomState state)
                State = state;
        });
    }

    private void Commands()
    {
        Command<IChatRoomCommand>(cmd =>
        {
            var (resultType, events) = State.Process(cmd);

            switch (resultType)
            {
                case CommandResultType.Failure:
                    Sender.Tell(CommandResult.Failure($"Failed to process command {cmd}"));
                    break;
                case CommandResultType.NoOp:
                    Sender.Tell(CommandResult.NoOp());
                    break;
            }
            
            var sentReply = false;
            
            PersistAll(events, evt =>
            {
                State = State.Apply(evt);
                _log.Info("Persisted event {0}", evt);
                
                // TODO: need special handling for when a message is posted to chatroom

                if (!sentReply)
                {
                    Sender.Tell(CommandResult.Success());
                    sentReply = true;
                }
            });
        });
    }
}