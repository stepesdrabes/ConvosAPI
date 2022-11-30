using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Convos.API.Data.Entities;

public class ConversationUser
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key, JsonIgnore]
    public int Id { get; set; }

    public string? Nickname { get; set; }
    public string UserId { get; set; }
    [ForeignKey("UserId")] public User User { get; set; }
    public string ConversationId { get; set; }

    [JsonIgnore, ForeignKey("ConversationId")]
    public Conversation Conversation { get; set; }

    [JsonIgnore] public bool Deleted { get; set; }
    public DateTime AddedAt { get; set; }
}