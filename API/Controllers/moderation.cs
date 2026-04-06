using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/guilds/{guildId}/moderation")]
public class ModerationController : ControllerBase
{
    private readonly GuildConfigService _guildConfigService;

    public ModerationController(GuildConfigService guildConfigService)
    {
        _guildConfigService = guildConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> GetModeration(ulong guildId)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        return Ok(new
        {
            welcomeConfig = config.Welcome ?? new WelcomeConfig(),
            leaveConfig = config.Leave ?? new LeaveConfig(),
            roleConfig = config.Roles ?? new RoleConfig()
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateModeration(ulong guildId, [FromBody] ModerationDto dto)
    {
        if (dto == null)
            return BadRequest("Payload inválido.");

        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        config.Welcome = dto.WelcomeConfig ?? new WelcomeConfig();
        config.Leave = dto.LeaveConfig ?? new LeaveConfig();
        config.Roles = dto.RoleConfig ?? new RoleConfig();

        await _guildConfigService.UpdateConfig(config);

        return Ok(new { success = true });
    }
}