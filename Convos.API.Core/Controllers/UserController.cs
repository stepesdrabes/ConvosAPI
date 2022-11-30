using Convos.API.Core.Services;
using Convos.API.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core.Controllers;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class UserController : AuthControllerBase
{
    public UserController(UserService userService) : base(userService)
    {
    }

    [HttpGet]
    [Route("users")]
    public async Task<ActionResult<List<User>>> GetAllUsers() => Ok(await UserService.GetAllUsers());

    [HttpGet]
    [Route("user")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        return Ok(currentUser);
    }

    [HttpGet]
    [Route("user/{id}")]
    public async Task<ActionResult<User>> GetUser(string id)
    {
        var user = await UserService.GetUserById(id);

        return user == null ? NotFound() : Ok(user);
    }
}