using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Convos.API.Data.Entities;

public class Conversation
{
    [Key] public string Id { get; set; }
    public string Name { get; set; }
    public string? ImageName { get; set; }
    public string OwnerId { get; set; }
    [ForeignKey("OwnerId")] public User Owner { get; set; }
    public List<ConversationUser> ConversationUsers { get; set; }
    public DateTime CreatedAt { get; set; }
}