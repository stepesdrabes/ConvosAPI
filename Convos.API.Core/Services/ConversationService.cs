using Convos.API.Core.Hubs;
using Convos.API.Data;
using Convos.API.Data.Entities;
using Convos.API.Data.Models.Conversations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Convos.API.Core.Services;

public class ConversationService
{
    private readonly DatabaseContext _context;
    private readonly FileService _fileService;
    private readonly UserService _userService;
    private readonly IHubContext<MessagesHub> _messagesHub;

    public ConversationService(DatabaseContext context, FileService fileService, UserService userService,
        IHubContext<MessagesHub> messagesHub)
    {
        _context = context;
        _fileService = fileService;
        _userService = userService;
        _messagesHub = messagesHub;
    }

    private async Task<Conversation?> GetConversationById(string conversationId)
    {
        return await _context.Conversations
            .Include(x => x.Owner)
            .Include(x => x.ConversationUsers)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == conversationId);
    }

    public async Task<Conversation> CreateConversation(User user, CreateConversationModel model)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid().ToString(),
            Name = model.Name,
            Owner = user,
            CreatedAt = DateTime.Now
        };

        if (model.ImageFile != null)
        {
            conversation.ImageName = await _fileService.UploadFile(model.ImageFile);
        }

        var users = new List<User>();

        model.Users.Add(user.Id);

        foreach (var userId in model.Users.Distinct())
        {
            var conversationUser = await _userService.GetUserById(userId);
            if (conversationUser == null) continue;
            users.Add(conversationUser);
        }

        var conversationUsers = users.Distinct().Select(x => new ConversationUser
        {
            User = x,
            Conversation = conversation,
            ConversationId = conversation.Id,
            AddedAt = DateTime.Now
        }).ToList();

        await _context.AddAsync(conversation);
        await _context.AddRangeAsync(conversationUsers);
        await _context.SaveChangesAsync();

        conversation.ConversationUsers = conversationUsers;

        foreach (var connectionId in conversation.ConversationUsers.SelectMany(conversationUser =>
                     MessagesHub.Connections.GetConnections(conversationUser.ConversationId)))
        {
            await _messagesHub.Clients.Client(connectionId).SendAsync("ConversationCreated", conversation);
        }

        return conversation;
    }

    public async Task<Conversation> UpdateConversation(Conversation conversation, UpdateConversationModel model)
    {
        conversation.Name = model.Name ?? conversation.Name;

        if (model.ImageFile != null)
        {
            if (conversation.ImageName != null) _fileService.DeleteFile(conversation.ImageName);
            conversation.ImageName = await _fileService.UploadFile(model.ImageFile);
        }

        if (model.RemoveImage == true && conversation.ImageName != null)
        {
            _fileService.DeleteFile(conversation.ImageName);
        }

        _context.Conversations.Update(conversation);
        await _context.SaveChangesAsync();

        return conversation;
    }

    public async Task DeleteConversation(Conversation conversation)
    {
        _context.Conversations.Remove(conversation);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Conversation>> GetUserConversations(User user)
    {
        return await _context.Conversations
            .Include(conversation => conversation.ConversationUsers)
            .ThenInclude(x => x.User)
            .Include(x => x.Owner)
            .Where(x => x.OwnerId == user.Id ||
                        x.ConversationUsers.FirstOrDefault(y => y.UserId == user.Id && !y.Deleted) != null)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool, Conversation?, ConversationUser?)> IsConversationVisible(User user, string conversationId)
    {
        var conversation = await GetConversationById(conversationId);
        if (conversation == null) return (false, null, null);

        var conversationUser =
            conversation.ConversationUsers.FirstOrDefault(x =>
                x.ConversationId == conversationId && x.UserId == user.Id);

        return conversationUser == null ? (false, null, null) : (true, conversation, conversationUser);
    }
}