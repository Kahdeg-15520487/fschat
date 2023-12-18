using be.Controllers.DTOs;
using be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace be.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        [HttpGet("username/{userId}")]
        public async Task<IActionResult> GetUserName([FromQuery] Guid userId)
        {
            var userName = await _chatService.GetUserName(userId);
            return Ok(new GetUserNameResponse { UserName = userName });
        }

        [HttpGet("messages")]   
        public async Task<IActionResult> GetMessages([FromQuery] GetMessagesRequest request)
        {
            var messages = await _chatService.GetMessages(request.GroupId);
            return Ok(new GetMessagesResponse { Messages = messages });
        }
    }
}
