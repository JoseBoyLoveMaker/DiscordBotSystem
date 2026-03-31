using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("guilds")]
public class GuildsController : ControllerBase
{
    private const string SessionCookieName = "botpanel_session";

    private readonly UserSessionStore _sessionStore;
    private readonly DiscordAuthService _discordAuthService;

    public GuildsController(UserSessionStore sessionStore, DiscordAuthService discordAuthService)
    {
        _sessionStore = sessionStore;
        _discordAuthService = discordAuthService;
    }

    [HttpGet]
    public async Task<IActionResult> GetGuilds(CancellationToken cancellationToken)
    {
        var sessionId = Request.Cookies[SessionCookieName];
        if (string.IsNullOrWhiteSpace(sessionId))
            return Unauthorized();

        var user = _sessionStore.Get(sessionId);
        if (user == null)
            return Unauthorized();

        var guilds = await _discordAuthService.GetCurrentUserGuildsAsync(user.AccessToken, cancellationToken);

        var result = guilds
            .Where(g => HasManageGuildPermission(g.Permissions) || g.Owner)
            .Select(g => new
            {
                id = g.Id,
                name = g.Name,
                icon = g.Icon,
                owner = g.Owner,
                permissions = g.Permissions
            })
            .ToList();

        return Ok(result);
    }

    private static bool HasManageGuildPermission(ulong permissions)
    {
        const ulong ManageGuild = 1UL << 5;
        const ulong Administrator = 1UL << 3;

        return (permissions & ManageGuild) == ManageGuild ||
               (permissions & Administrator) == Administrator;
    }
}