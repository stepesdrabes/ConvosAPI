using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Convos.API.Data.Models.Conversations;

public class CreateConversationModel
{
    [MaxLength(32)] public string Name { get; set; }
    public IFormFile? ImageFile { get; set; }
    public List<string> Users { get; set; }
}