using Convos.API.Core.Hubs;
using Convos.API.Data;
using Convos.API.Data.Entities;
using Convos.API.Data.Models.Conversations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Convos.API.Core.Services;

public class ConversationMessageService
{
    private readonly DatabaseContext _context;
    private readonly IHubContext<MessagesHub> _messagesHub;

    public ConversationMessageService(DatabaseContext context, IHubContext<MessagesHub> messagesHub)
    {
        _context = context;
        _messagesHub = messagesHub;
    }

    public async Task<ConversationMessage> CreateConversationMessage(ConversationUser author, Conversation conversation,
        CreateConversationMessageModel model)
    {
        var message = new ConversationMessage
        {
            Id = Guid.NewGuid().ToString(),
            TextContent = model.TextContent,
            Author = author,
            Conversation = conversation,
            CreatedAt = DateTime.Now
        };

        await _context.ConversationMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        
        foreach (var connectionId in conversation.ConversationUsers.SelectMany(conversationUser =>
                     MessagesHub.Connections.GetConnections(conversationUser.ConversationId)))
        {
            await _messagesHub.Clients.Client(connectionId).SendAsync("MessageCreated", message);
        }
        
        return message;
    }

    public async Task<List<ConversationMessage>> GetConversationMessages(string conversationId)
    {
        return await _context.ConversationMessages
            .Include(x => x.Author)
            .ThenInclude(x => x.User)
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<ConversationMessage?> GetLatestConversationMessage(string conversationId)
    {
        return await _context.ConversationMessages
            .Include(x => x.Author)
            .ThenInclude(x => x.User)
            .Where(x => x.ConversationId == conversationId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<ConversationMessage?> GetConversationMessageById(string conversationId, string messageId)
    {
        return await _context.ConversationMessages
            .Include(x => x.Author)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == messageId && x.ConversationId == conversationId);
    }

    public async Task DeleteMessage(ConversationMessage conversationMessage)
    {
        _context.Remove(conversationMessage);
        await _context.SaveChangesAsync();
    }
}