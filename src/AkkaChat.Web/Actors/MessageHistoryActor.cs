// -----------------------------------------------------------------------
//  <copyright file="MessageHistoryActor.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;
using Akka.Persistence;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Models;
using static AkkaChat.Models.PersistenceIdGenerators;

namespace AkkaChat.Web.Actors;

public sealed class MessageHistoryActor : ReceivePersistentActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly HashSet<IActorRef> _subscribers = new();

    public MessageHistoryActor(string chatroomId)
    {
        PersistenceId = IdForChatRoom(chatroomId);

        Recovers();
        Commands();
    }

    public ChatRoomState State { get; private set; } = ChatRoomState.Empty;
    public override string PersistenceId { get; }

    private void Recovers()
    {
        Recover<IChatRoomEvent>(@event => { State = State.Apply(@event); });

        Recover<SnapshotOffer>(offer =>
        {
            if (offer.Snapshot is ChatRoomState state)
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

                if (evt is not ChatRoomEvents.ChatRoomMessagePosted messagePosted) return;

                // publish chatroom messages to all subscribers
                foreach (var subscriber in _subscribers)
                    subscriber.Tell(messagePosted);
            });
        });

        Command<ChatRoomQueries.GetRecentMessages>(get =>
        {
            Sender.Tell(State.RecentMessages.Take(Math.Min(State.RecentMessages.Count, get.Count))
                .ToImmutableSortedSet());
        });

        Command<ChatRoomQueries.SubscribeToMessages>(sub =>
        {
            _subscribers.Add(Sender);
            Context.WatchWith(Sender, new ChatRoomQueries.UnsubscribeFromMessages(sub.ChatRoomId, Sender));
        });

        Command<ChatRoomQueries.UnsubscribeFromMessages>(unsub =>
        {
            _subscribers.Remove(Sender);
            Context.Unwatch(Sender);
        });
    }
}