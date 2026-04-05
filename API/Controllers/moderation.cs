using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/guilds/{guildId}/moderation")]
public class GuildModerationController : ControllerBase
{
    private readonly GuildConfigService _guildConfigService;

    public GuildModerationController(GuildConfigService guildConfigService)
    {
        _guildConfigService = guildConfigService;
    }

    [HttpGet]
    public async Task<IActionResult> GetModeration(ulong guildId)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        return Ok(new
        {
            welcome = config.Welcome,
            leave = config.Leave,
            roles = config.Roles
        });
    }

    [HttpPut("welcome")]
    public async Task<IActionResult> UpdateWelcome(ulong guildId, [FromBody] UpdateWelcomeConfigDto dto)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        config.Welcome ??= new WelcomeConfig();
        config.Welcome.Enabled = dto.Enabled;
        config.Welcome.ChannelId = dto.ChannelId;
        config.Welcome.Message = dto.Message;
        config.Welcome.AutoRoleId = dto.AutoRoleId;

        await _guildConfigService.UpdateConfig(config);

        return Ok(config.Welcome);
    }

    [HttpPut("leave")]
    public async Task<IActionResult> UpdateLeave(ulong guildId, [FromBody] UpdateLeaveConfigDto dto)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        config.Leave ??= new LeaveConfig();
        config.Leave.Enabled = dto.Enabled;
        config.Leave.ChannelId = dto.ChannelId;
        config.Leave.Message = dto.Message;

        await _guildConfigService.UpdateConfig(config);

        return Ok(config.Leave);
    }

    [HttpPut("roles")]
    public async Task<IActionResult> UpdateRoles(ulong guildId, [FromBody] UpdateRoleConfigDto dto)
    {
        var config = await _guildConfigService.GetOrCreateConfig(guildId);

        config.Roles ??= new RoleConfig();
        config.Roles.LevelRoles = dto.LevelRoles.Select(x => new LevelRoleReward
        {
            RoleId = x.RoleId,
            MinChatLevel = x.MinChatLevel,
            MinCallLevel = x.MinCallLevel
        }).ToList();

        await _guildConfigService.UpdateConfig(config);

        return Ok(config.Roles);
    }
}