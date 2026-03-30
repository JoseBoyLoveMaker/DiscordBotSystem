using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/commands")]
    public class CommandsController : ControllerBase
    {
        private readonly CommandServiceAPI _commands;

        public CommandsController(CommandServiceAPI commands)
        {
            _commands = commands;
        }

        [HttpGet("{guildId}")]
        public async Task<IActionResult> GetAll(ulong guildId)
        {
            var data = await _commands.GetCommandsByGuild(guildId);
            return Ok(data);
        }

        [HttpPut("{guildId}/{commandName}/enabled")]
        public async Task<IActionResult> SetEnabled(
            ulong guildId,
            string commandName,
            [FromBody] bool enabled)
        {
            await _commands.SetEnabled(guildId, commandName, enabled);
            return Ok();
        }

        [HttpPut("{guildId}/{commandName}/aliases")]
        public async Task<IActionResult> UpdateAliases(
            ulong guildId,
            string commandName,
            [FromBody] List<string> aliases)
        {
            await _commands.UpdateAliases(guildId, commandName, aliases);
            return Ok();
        }

        [HttpPut("{guildId}/{commandName}/cooldown")]
        public async Task<IActionResult> UpdateCooldown(
            ulong guildId,
            string commandName,
            [FromBody] int cooldown)
        {
            await _commands.UpdateCooldown(guildId, commandName, cooldown);
            return Ok();
        }
    }
}