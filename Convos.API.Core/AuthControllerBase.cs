using System.Security.Claims;
using Convos.API.Core.Services;
using Convos.API.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core;

public class AuthControllerBase : ControllerBase
{
    protected readonly UserService UserService;

    public AuthControllerBase(UserService userService)
    {
        UserService = userService;
    }

    protected async Task<User?> GetAuthenticatedUser()
    {
        var claimsIdentity = User.Identity as ClaimsIdentity;
        var usernameClaim = claimsIdentity?.FindFirst(ClaimTypes.Name);

        if (usernameClaim == null) return null;

        return await UserService.GetUserByUsername(usernameClaim.Value);
    }
}