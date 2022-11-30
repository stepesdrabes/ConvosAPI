using System.Security.Claims;
using Convos.API.Core.Services;
using Convos.API.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Convos.API.Core.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OnlineHub : Hub
{
    private static readonly ConnectionMapping<string> Connections = new();

    private readonly UserService _userService;
    private readonly ILogger<OnlineHub> _logger;

    public OnlineHub(UserService userService, ILogger<OnlineHub> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var user = await GetCurrentUser();

        if (user == null)
        {
            Context.Abort();
            return;
        }

        _logger.LogInformation("User {UserId} connected to online hub", user.Id);
        Connections.Add(user.Id, Context.ConnectionId);

        await UserConnected(user.Id);

        await Task.Delay(500);
        await Clients.Client(Context.ConnectionId).SendAsync("OnlineUsers", Connections.GetConnectedUsers());
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await GetCurrentUser();

        if (user == null)
        {
            Context.Abort();
            return;
        }

        _logger.LogInformation("User {UserId} disconnected from online hub", user.Id);
        Connections.Remove(user.Id, Context.ConnectionId);

        await UserDisconnected(user.Id);
    }

    private async Task<User?> GetCurrentUser()
    {
        var claimsIdentity = Context.User?.Identity as ClaimsIdentity;
        var usernameClaim = claimsIdentity?.FindFirst(ClaimTypes.Name);
        if (usernameClaim == null) return null;

        return await _userService.GetUserByUsername(usernameClaim.Value);
    }

    private async Task UserConnected(string userId) => await Clients.All.SendAsync("UserConnected", userId);
    private async Task UserDisconnected(string userId) => await Clients.All.SendAsync("UserDisconnected", userId);
}