using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/bot")]
    public class BotController : ControllerBase
    {
        private readonly UserServiceAPI _users;
        private readonly BotStatusServiceAPI _botStatus;

        public BotController(UserServiceAPI users, BotStatusServiceAPI botStatus)
        {
            _users = users;
            _botStatus = botStatus;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var users = await _users.CountUsers();
            var totalXp = await _users.TotalXp();
            var botStatus = await _botStatus.GetStatus();
            bool isOnline = botStatus?.IsOnline ?? false;

            return Ok(new
            {
                users,
                totalXp,
                commands = 1204,
                status = isOnline ? "Online" : "Offline"
            });
        }
    }
}