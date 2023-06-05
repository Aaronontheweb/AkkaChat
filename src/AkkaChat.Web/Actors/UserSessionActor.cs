// -----------------------------------------------------------------------
//  <copyright file="UserSessionActor.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2023 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2023 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Actor;
using Akka.Event;
using Akka.Hosting;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Messages.Users;
using AkkaChat.Models;

namespace AkkaChat.Web.Actors;

public sealed class UserSessionActor : ReceiveActor, IWithStash, IWithTimers
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    public UserSessionState State { get; private set; } = UserSessionState.Empty;
    private readonly IActorRef _chatRoomActors;

    private sealed class CreateTimeout
    {
        public static readonly CreateTimeout Instance = new();
        private CreateTimeout(){}
    }

    public UserSessionActor(string userId, IRequiredActor<RoomManagerActor> chatRoomActors)
    {
        _chatRoomActors = chatRoomActors.ActorRef;
        State = State with { UserId = userId };
        
        WaitingToBeCreated();
        Timers!.StartSingleTimer("create-timeout", CreateTimeout.Instance, TimeSpan.FromSeconds(5));
    }

    private void WaitingToBeCreated()
    {
        ReceiveAsync<UserSessionCommands.CreateSession>(async create =>
        {
            var (resultType, events) = await State.ProcessAsync(create, _chatRoomActors, CancellationToken.None);
            
            switch (resultType)
            {
                case CommandResultType.Failure:
                    Sender.Tell(CommandResult.Failure($"Failed to process command {create}"));
                    break;
                case CommandResultType.NoOp:
                    Sender.Tell(CommandResult.NoOp());
                    break;
                case CommandResultType.Success:
                    Sender.Tell(CommandResult.Success());
                    break;
            }
            
            foreach (var @event in events)
            {
                _log.Info("UserSessionActor: Processing {0}", @event);
                State = State.Apply(@event);
            }
            
            if(State.IsEmpty)
                throw new InvalidOperationException("UserSessionState is empty - should have been created");
            
            Timers.Cancel("create-timeout"); // turn off our create timeout
            Become(Active);
            Stash.UnstashAll();
            Context.SetReceiveTimeout(TimeSpan.FromMinutes(30));
        });

        Receive<CreateTimeout>(_ =>
        {
            _log.Error("UserSessionActor: CreateTimeout after 5s - stopping actor");
            Stash.UnstashAll(); // need to NACK all messages that have been buffered
            Self.Tell(PoisonPill.Instance); // only shutdown after all stashed messages have been processed
            Become(TimedOut);
        });
        
        ReceiveAny(_ =>
        {
            // buffer any messages we can't process right now
            Stash.Stash();
        });
    }

    private void Active()
    {
        ReceiveAsync<IUserSessionCommand>(async cmd =>
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            var (resultType, events) = await State.ProcessAsync(cmd, _chatRoomActors, cts.Token);

            switch (resultType)
            {
                case CommandResultType.Failure:
                    Sender.Tell(CommandResult.Failure($"Failed to process command {cmd}"));
                    break;
                case CommandResultType.NoOp:
                    Sender.Tell(CommandResult.NoOp());
                    break;
            }

            foreach (var @event in events)
            {
                _log.Info("UserSessionActor: Processing {0}", @event);
                
                State = State.Apply(@event);
            }

            Sender.Tell(CommandResult.Success());
        });

        Receive<UserSessionQueries.GetSessionState>(state =>
        {
            Sender.Tell(State);
        });

        Receive<ReceiveTimeout>(_ => Context.Stop(Self));
    }

    private void TimedOut()
    {
        Receive<IUserSessionCommand>(cmd =>
        {
            Sender.Tell(CommandResult.Failure($"Failed to process command {cmd} - Session failed to create after 5s."));
        });
    }

    public IStash Stash { get; set; } = null!;
    public ITimerScheduler Timers { get; set; }
}