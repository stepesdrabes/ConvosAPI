using Microsoft.AspNetCore.Http;

namespace Convos.API.Data.Models.Conversations;

public class UpdateConversationModel
{
    public string? Name { get; set; }
    public IFormFile? ImageFile { get; set; }
    public bool? RemoveImage { get; set; }
}