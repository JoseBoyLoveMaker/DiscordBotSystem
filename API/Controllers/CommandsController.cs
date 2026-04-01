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

        [HttpGet("{guildId}/{commandName}")]
        public async Task<IActionResult> GetOne(ulong guildId, string commandName)
        {
            var data = await _commands.GetCommandByGuild(guildId, commandName);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPut("{guildId}/{commandName}/enabled")]
        public async Task<IActionResult> SetEnabled(
            ulong guildId,
            string commandName,
            [FromBody] bool enabled)
        {
            var ok = await _commands.SetEnabled(guildId, commandName, enabled);

            if (!ok)
                return NotFound("Comando não encontrado.");

            return Ok();
        }

        [HttpPut("{guildId}/{commandName}/aliases")]
        public async Task<IActionResult> UpdateAliases(
            ulong guildId,
            string commandName,
            [FromBody] List<string> aliases)
        {
            var ok = await _commands.UpdateAliases(guildId, commandName, aliases);

            if (!ok)
                return NotFound("Comando não encontrado.");

            return Ok();
        }

        [HttpPut("{guildId}/{commandName}/cooldown")]
        public async Task<IActionResult> UpdateCooldown(
            ulong guildId,
            string commandName,
            [FromBody] int cooldown)
        {
            var ok = await _commands.UpdateCooldown(guildId, commandName, cooldown);

            if (!ok)
                return NotFound("Comando não encontrado.");

            return Ok();
        }
    }
}