using Microsoft.AspNetCore.Http;

namespace Convos.API.Data.Models.Auth;

public class RegisterModel : LoginModel
{
    public IFormFile? ImageFile { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}