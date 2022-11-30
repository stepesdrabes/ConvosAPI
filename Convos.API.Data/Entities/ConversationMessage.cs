using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Convos.API.Data.Entities;

public class ConversationMessage
{
    [Key] public string Id { get; set; }
    public string TextContent { get; set; }
    public string? ImageName { get; set; }
    [ForeignKey("AuthorId")] public ConversationUser Author { get; set; }
    [JsonIgnore] public int AuthorId { get; set; }

    [JsonIgnore, ForeignKey("ConversationId")]
    public Conversation Conversation { get; set; }

    public string ConversationId { get; set; }

    [JsonIgnore] public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}