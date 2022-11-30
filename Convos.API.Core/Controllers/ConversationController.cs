using Convos.API.Core.Services;
using Convos.API.Data.Models.Conversations;
using Convos.API.Data.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core.Controllers;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ConversationController : AuthControllerBase
{
    private readonly ConversationService _conversationService;
    private readonly ConversationMessageService _conversationMessageService;

    public ConversationController(UserService userService, ConversationService conversationService,
        ConversationMessageService conversationMessageService) : base(userService)
    {
        _conversationService = conversationService;
        _conversationMessageService = conversationMessageService;
    }

    #region Conversations

    [HttpGet]
    [Route("conversations")]
    public async Task<ActionResult<List<ConversationResponse>>> GetAllConversations()
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var conversations = await _conversationService.GetUserConversations(currentUser);

        var conversationResponses = new List<ConversationResponse>();
        foreach (var conversation in conversations)
        {
            var latestMessage = await _conversationMessageService.GetLatestConversationMessage(conversation.Id);
            conversationResponses.Add(ConversationResponse.FromEntity(conversation, latestMessage));
        }

        return Ok(conversationResponses.OrderByDescending(x => x.LastMessage?.CreatedAt ?? x.CreatedAt));
    }

    [HttpPost]
    [Route("conversations")]
    public async Task<ActionResult<ConversationResponse>> CreateConversation([FromForm] CreateConversationModel model)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var addableUsers = model.Users.Where(x => x != currentUser.Id).Distinct().Count();
        if (addableUsers == 0) return BadRequest();

        model = new CreateConversationModel
        {
            Name = model.Name,
            ImageFile = model.ImageFile,
            Users = model.Users.Select(x => x.Substring(1, x.Length - 2)).ToList()
        };

        var conversation = await _conversationService.CreateConversation(currentUser, model);
        var latestMessage = await _conversationMessageService.GetLatestConversationMessage(conversation.Id);
        return Ok(ConversationResponse.FromEntity(conversation, latestMessage));
    }

    #endregion

    #region Conversation

    [HttpGet]
    [Route("conversation/{conversationId}")]
    public async Task<ActionResult<ConversationResponse>> GetConversationById(string conversationId)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, _) = await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null) return NotFound();
        if (!visible) return Forbid();

        var latestMessage = await _conversationMessageService.GetLatestConversationMessage(conversation.Id);
        return Ok(ConversationResponse.FromEntity(conversation, latestMessage));
    }

    [HttpPost]
    [Route("conversation/{conversationId}")]
    public async Task<ActionResult<ConversationResponse>> UpdateConversation(string conversationId,
        [FromBody] UpdateConversationModel model)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, _) = await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null) return NotFound();
        if (!visible) return Forbid();

        conversation = await _conversationService.UpdateConversation(conversation, model);
        var latestMessage = await _conversationMessageService.GetLatestConversationMessage(conversation.Id);
        return Ok(ConversationResponse.FromEntity(conversation, latestMessage));
    }

    [HttpDelete]
    [Route("conversation/{conversationId}")]
    public async Task<ActionResult> DeleteConversation(string conversationId)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, _) = await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null) return NotFound();
        if (!visible) return Forbid();

        await _conversationService.DeleteConversation(conversation);
        return Ok();
    }

    #endregion
}