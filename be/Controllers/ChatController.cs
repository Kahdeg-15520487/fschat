using be.Controllers.DTOs;
using be.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace be.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService chatService;
        private readonly IPdfService pdfService;

        public ChatController(IChatService chatService, IPdfService pdfService)
        {
            this.chatService = chatService;
            this.pdfService = pdfService;
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
            var userName = await chatService.GetUserName(UserId);
            return Ok(new GetUserNameResponse { UserName = userName });
        }

        [HttpPost("username")]
        public async Task<IActionResult> SetUserName([FromBody] SetUserNameRequest dto)
        {
            await chatService.SetUserName(dto.UserId, dto.UserName);
            return Ok();
        }

        [HttpGet("message")]
        public async Task<IActionResult> GetMessage([FromQuery] Guid messageId)
        {
            return Ok(await chatService.GetMessage(messageId));
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages([FromQuery] Guid GroupId)
        {
            var messages = await chatService.GetMessages(GroupId);
            return Ok(new GetMessagesResponse { Messages = messages });
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessages([FromBody] MessageObject message)
        {
            return Ok(await chatService.SendMessage(message.userId, Guid.Parse(message.roomId), message.message));
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> GeneratePdf([FromQuery] Guid GroupId)
        {
            var stream = await pdfService.GeneratePdfFromChat(GroupId);
            return new FileStreamResult(stream, "application/pdf") { FileDownloadName = $"exported_chat_{DateTime.UtcNow}" };
        }
    }
}
