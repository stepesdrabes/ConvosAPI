using System.Web.Helpers;
using Convos.API.Core.Services;
using Convos.API.Data.Models.Auth;
using Convos.API.Data.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : AuthControllerBase
{
    private readonly TokenService _tokenService;

    public AuthController(UserService userService, TokenService tokenService) : base(userService)
    {
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("signin")]
    public async Task<ActionResult<AccessTokenResponse>> Login([FromBody] LoginModel model)
    {
        var user = await UserService.GetUserByUsername(model.Username);
        if (user == null) return BadRequest();

        if (!Crypto.VerifyHashedPassword(user.PasswordHash, model.Password)) return BadRequest();

        return Ok(_tokenService.GenerateAccessToken(user));
    }

    [HttpPost]
    [Route("signup")]
    public async Task<ActionResult<AccessTokenResponse>> Register([FromForm] RegisterModel model)
    {
        var user = await UserService.GetUserByUsername(model.Username);
        if (user != null) return BadRequest();

        if (model.ImageFile != null && !FileService.IsValidImage(model.ImageFile)) return BadRequest();

        user = await UserService.CreateUser(model);

        return Ok(_tokenService.GenerateAccessToken(user));
    }
}