using System.Security.Claims;
using Convos.API.Core.Services;
using Convos.API.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Convos.API.Core.Hubs;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MessagesHub : Hub
{
    public static readonly ConnectionMapping<string> Connections = new();

    private readonly UserService _userService;
    private readonly ILogger<MessagesHub> _logger;

    public MessagesHub(UserService userService, ILogger<MessagesHub> logger)
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

        _logger.LogInformation("User {UserId} connected to messages hub", user.Id);
        Connections.Add(user.Id, Context.ConnectionId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = await GetCurrentUser();

        if (user == null)
        {
            Context.Abort();
            return;
        }

        _logger.LogInformation("User {UserId} disconnected from messages hub", user.Id);
        Connections.Remove(user.Id, Context.ConnectionId);
    }

    private async Task<User?> GetCurrentUser()
    {
        var claimsIdentity = Context.User?.Identity as ClaimsIdentity;
        var usernameClaim = claimsIdentity?.FindFirst(ClaimTypes.Name);
        if (usernameClaim == null) return null;

        return await _userService.GetUserByUsername(usernameClaim.Value);
    }
}