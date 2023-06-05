using Akka.Actor;
using Akka.Hosting;
using AkkaChat.Web.Actors;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using AkkaChat.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAkka("AkkaChat", (configurationBuilder, provider) =>
{
    configurationBuilder
        .WithActors((system, registry, _) =>
        {
            IActorRef roomManager = system.ActorOf(Props.Create(() => new RoomManagerActor()), "rooms");
            registry.Register<RoomManagerActor>(roomManager);
        })
        .WithActors((system, registry, resolver) =>
        {
            // populate constructor arguments for actor using the IServiceProvider (DI)
            Props userSessionManagerProps = resolver.Props<UserSessionManager>();
            IActorRef userSessionManager = system.ActorOf(userSessionManagerProps, "user-session-manager");
            registry.Register<UserSessionManager>(userSessionManager);
        });
});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
