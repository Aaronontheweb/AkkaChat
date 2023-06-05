// -----------------------------------------------------------------------
//  <copyright file="UserSessionActorSpecs.cs" company="Akka.NET Project">
//      Copyright (C) 2015-2023 .NET Petabridge, LLC
//  </copyright>
// -----------------------------------------------------------------------

using Akka.Hosting;
using Akka.Hosting.TestKit;
using AkkaChat.Messages.ChatRooms;
using AkkaChat.Messages.Users;
using AkkaChat.Models;
using AkkaChat.Web.Actors;
using AkkaChat.Web.Config;
using FluentAssertions;
using Xunit.Abstractions;

namespace AkkaChat.Web.Tests;

public class UserSessionActorSpecs : TestKit
{
    public UserSessionActorSpecs(ITestOutputHelper outputHelper) : base(nameof(UserSessionActorSpecs), outputHelper)
    {
    }

    protected override void ConfigureAkka(AkkaConfigurationBuilder builder, IServiceProvider provider)
    {
        builder.AddAkkaChatActors();
    }

    [Fact]
    public async Task UserSessionActorShouldJoinAndLeaveChatroomHappyPath()
    {
        // arrange
        var userId = "aaron";
        var chatRoomId = "akka-chat";
        IChatRoomCommand createChatRoom = new ChatRoomCommands.CreateChatRoom(chatRoomId, "Akka.NET Chat", userId);
        var chatRoomManager = await ActorRegistry.GetAsync<RoomManagerActor>();
        chatRoomManager.Tell(createChatRoom, TestActor);

        // chat room should be created successfully
        (await ExpectMsgAsync<CommandResult>()).Type.Should().Be(CommandResultType.Success);

        IUserSessionCommand[] commandsForUser =
        {
            new UserSessionCommands.CreateSession(userId, "Aaronontheweb"),
            new UserSessionCommands.JoinChatRoom(userId, chatRoomId)
        };

        var leaveCommand = new UserSessionCommands.LeaveChatRoom(userId, chatRoomId);

        var userSessionManager = await ActorRegistry.GetAsync<UserSessionManager>();

        // act

        // populate user and join chatroom
        foreach (var c in commandsForUser) userSessionManager.Tell(c, TestActor);

        // create command
        (await ExpectMsgAsync<CommandResult>()).Type.Should().Be(CommandResultType.Success);

        // join command
        (await ExpectMsgAsync<CommandResult>()).Type.Should().Be(CommandResultType.Success);
    }
}