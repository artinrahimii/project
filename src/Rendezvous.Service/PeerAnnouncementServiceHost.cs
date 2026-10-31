using System;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using WhiteP.Shared.Contracts.Networking;
namespace WhiteP.Rendezvous.Service;

public static class PeerAnnouncementServiceHost
{
    public static WebApplication Create(string[]? args = null, string? urlsOverride = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args ?? Array.Empty<string>(),
            ApplicationName = typeof(PeerAnnouncementServiceHost).Assembly.GetName().Name
        });
        if (!string.IsNullOrWhiteSpace(urlsOverride)) builder.WebHost.UseUrls(urlsOverride);
        builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = PeerAnnouncementWireFormat.MaximumMessageBytes);
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.MaxDepth = 8;
            options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        });
        builder.Services.AddSingleton<PeerAnnouncementDirectory>();
        var app = builder.Build();
        app.MapPost("/v1/peers/announcements", HandleAnnouncement);
        app.MapGet("/v1/peers", HandleCapabilityDiscovery);
        return app;
    }

    private static IResult HandleAnnouncement(PeerAnnouncement announcement, PeerAnnouncementDirectory directory)
    {
        try { directory.Announce(announcement); return Results.NoContent(); }
        catch (ArgumentException) { return Results.BadRequest(new { error = "invalid_announcement" }); }
        catch (InvalidOperationException) { return Results.StatusCode(StatusCodes.Status503ServiceUnavailable); }
    }

    private static IResult HandleCapabilityDiscovery(string? capability, PeerAnnouncementDirectory directory)
    {
        if (!PeerAnnouncement.IsValidCapabilityName(capability)) return Results.BadRequest(new { error = "invalid_capability" });
        try { return Results.Bytes(PeerAnnouncementWireFormat.SerializeDirectory(directory.DiscoverByCapability(capability!)), "application/json"); }
        catch (InvalidDataException) { return Results.StatusCode(StatusCodes.Status503ServiceUnavailable); }
    }
}
