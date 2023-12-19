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

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Ping()
        {
            return Ok("pong");
        }

        [HttpGet("username")]
        public async Task<IActionResult> GetUserName([FromQuery] string UserId)
        {
            var userName = await _chatService.GetUserName(UserId);
            return Ok(new GetUserNameResponse { UserName = userName });
        }

        [HttpPost("username")]
        public async Task<IActionResult> SetUserName([FromBody] SetUserNameRequest dto)
        {
            await _chatService.SetUserName(dto.UserId, dto.UserName);
            return Ok();
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] GetMessagesRequest request)
        {
            var messages = await _chatService.GetMessages(request.GroupId);
            return Ok(new GetMessagesResponse { Messages = messages });
        }
    }
}
