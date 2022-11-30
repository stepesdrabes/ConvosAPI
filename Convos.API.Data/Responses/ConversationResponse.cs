using Convos.API.Data.Entities;

namespace Convos.API.Data.Responses;

public class ConversationResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? ImageName { get; set; }
    public User Owner { get; set; }
    public List<ConversationUser> ConversationUsers { get; set; }
    public ConversationMessage? LastMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    public static ConversationResponse FromEntity(Conversation conversation, ConversationMessage? lastMessage)
    {
        return new ConversationResponse
        {
            Id = conversation.Id,
            Name = conversation.Name,
            ImageName = conversation.ImageName,
            Owner = conversation.Owner,
            ConversationUsers = conversation.ConversationUsers,
            LastMessage = lastMessage,
            CreatedAt = conversation.CreatedAt,
        };
    }
}