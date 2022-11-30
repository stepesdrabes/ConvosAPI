using Convos.API.Core.Services;
using Convos.API.Data.Entities;
using Convos.API.Data.Models.Conversations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Convos.API.Core.Controllers;

[ApiController]
[Route("api/messages/{conversationId}")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class ConversationMessageController : AuthControllerBase
{
    private readonly ConversationService _conversationService;
    private readonly ConversationMessageService _conversationMessageService;

    public ConversationMessageController(UserService userService, ConversationService conversationService,
        ConversationMessageService conversationMessageService) : base(userService)
    {
        _conversationService = conversationService;
        _conversationMessageService = conversationMessageService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationMessage>>> GetConversationMessages(string conversationId)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, _) = await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null) return NotFound();
        if (!visible) return Forbid();

        return Ok(await _conversationMessageService.GetConversationMessages(conversationId));
    }

    [HttpPost]
    public async Task<ActionResult<List<ConversationMessage>>> CreateConversationMessage(string conversationId,
        [FromBody] CreateConversationMessageModel model)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, conversationUser) =
            await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null || conversationUser == null) return NotFound();
        if (!visible) return Forbid();

        return Ok(await _conversationMessageService.CreateConversationMessage(conversationUser, conversation, model));
    }

    [HttpDelete]
    [Route("{messageId}")]
    public async Task<ActionResult> DeleteConversationMessage(string conversationId,
        string messageId)
    {
        var currentUser = await GetAuthenticatedUser();
        if (currentUser == null) return Unauthorized();

        var (visible, conversation, _) = await _conversationService.IsConversationVisible(currentUser, conversationId);
        if (conversation == null) return NotFound();
        if (!visible) return Forbid();

        var conversationMessage =
            await _conversationMessageService.GetConversationMessageById(conversationId, messageId);
        if (conversationMessage == null) return NotFound();
        if (conversationMessage.Author.User.Id != currentUser.Id) return Forbid();

        return Ok();
    }
}