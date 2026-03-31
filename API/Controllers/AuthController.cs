using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private const string SessionCookieName = "botpanel_session";
    private const string OAuthStateCookieName = "botpanel_oauth_state";

    private readonly DiscordAuthService _discordAuthService;
    private readonly DiscordOAuthSettings _oauthSettings;
    private readonly UserSessionStore _sessionStore;

    public AuthController(
        DiscordAuthService discordAuthService,
        DiscordOAuthSettings oauthSettings,
        UserSessionStore sessionStore)
    {
        _discordAuthService = discordAuthService;
        _oauthSettings = oauthSettings;
        _sessionStore = sessionStore;
    }

    [HttpGet("discord/login")]
    public IActionResult Login()
    {
        var state = Guid.NewGuid().ToString("N");

        Response.Cookies.Append(OAuthStateCookieName, state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(10)
        });

        var loginUrl = _discordAuthService.GetLoginUrl(state);
        return Redirect(loginUrl);
    }

    [HttpGet("discord/callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
    {
        var stateCookie = Request.Cookies[OAuthStateCookieName];

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state) || stateCookie != state)
        {
            return BadRequest("Callback OAuth inválido.");
        }

        var token = await _discordAuthService.ExchangeCodeAsync(code, cancellationToken);
        var user = await _discordAuthService.GetCurrentUserAsync(token.AccessToken, cancellationToken);

        var sessionId = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        _sessionStore.Set(sessionId, new AuthenticatedDiscordUser
        {
            DiscordId = user.Id,
            Username = user.Username,
            GlobalName = user.GlobalName,
            Avatar = user.Avatar,
            AvatarUrl = DiscordAuthService.BuildAvatarUrl(user),
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAtUtc = expiresAt
        });

        Response.Cookies.Append(SessionCookieName, sessionId, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt
        });

        Response.Cookies.Delete(OAuthStateCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Redirect(_oauthSettings.FrontEndUrl);
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        var user = GetCurrentSessionUser();
        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            id = user.DiscordId,
            username = user.Username,
            globalName = user.GlobalName,
            avatarUrl = user.AvatarUrl
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var sessionId = Request.Cookies[SessionCookieName];
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _sessionStore.Remove(sessionId);
        }

        Response.Cookies.Delete(SessionCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None
        });

        return Ok(new { success = true });
    }

    private AuthenticatedDiscordUser? GetCurrentSessionUser()
    {
        var sessionId = Request.Cookies[SessionCookieName];
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;

        return _sessionStore.Get(sessionId);
    }
}