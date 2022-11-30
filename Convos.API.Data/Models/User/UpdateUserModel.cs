using Microsoft.AspNetCore.Http;

namespace Convos.API.Data.Models.User;

public class UpdateUserModel
{
    public string? Username { get; set; }
    public IFormFile? ImageFile { get; set; }
}