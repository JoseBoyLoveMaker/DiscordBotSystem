using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class DiscordAuthService
{
    private const string AuthorizeUrl = "https://discord.com/oauth2/authorize";
    private const string TokenUrl = "https://discord.com/api/oauth2/token";
    private const string CurrentUserUrl = "https://discord.com/api/users/@me";
    private const string CurrentUserGuildsUrl = "https://discord.com/api/users/@me/guilds";

    private readonly HttpClient _httpClient;
    private readonly DiscordOAuthSettings _settings;

    public DiscordAuthService(HttpClient httpClient, DiscordOAuthSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;
    }

    public string GetLoginUrl(string state)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId))
            throw new Exception("DiscordOAuth.ClientId está vazio.");

        if (string.IsNullOrWhiteSpace(_settings.RedirectUri))
            throw new Exception("DiscordOAuth.RedirectUri está vazio.");

        if (string.IsNullOrWhiteSpace(state))
            throw new Exception("State do OAuth está vazio.");

        var scopes = "identify guilds";

        var query = new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = _settings.RedirectUri,
            ["scope"] = scopes,
            ["state"] = state
        };

        var queryString = string.Join("&",
            query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var url = $"{AuthorizeUrl}?{queryString}";
        Console.WriteLine("Discord login URL gerada: " + url);

        return url;
    }

    public async Task<DiscordTokenResponse> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _settings.RedirectUri
        });

        using var response = await _httpClient.PostAsync(TokenUrl, content, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao trocar code por token no Discord: {response.StatusCode} - {body}");
        }

        var token = JsonSerializer.Deserialize<DiscordTokenResponse>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (token == null || string.IsNullOrWhiteSpace(token.AccessToken))
        {
            throw new Exception("Resposta de token do Discord veio vazia ou inválida.");
        }

        return token;
    }

    public async Task<DiscordUserDto> GetCurrentUserAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, CurrentUserUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao buscar usuário do Discord: {response.StatusCode} - {body}");
        }

        var user = JsonSerializer.Deserialize<DiscordUserDto>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (user == null || string.IsNullOrWhiteSpace(user.Id))
        {
            throw new Exception("Usuário do Discord inválido.");
        }

        return user;
    }

    public async Task<List<DiscordGuildDto>> GetCurrentUserGuildsAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, CurrentUserGuildsUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Erro ao buscar guilds do usuário no Discord: {response.StatusCode} - {body}");
        }

        var guilds = JsonSerializer.Deserialize<List<DiscordGuildDto>>(body, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return guilds ?? new List<DiscordGuildDto>();
    }

    public static string? BuildAvatarUrl(DiscordUserDto user)
    {
        if (string.IsNullOrWhiteSpace(user.Avatar))
            return null;

        return $"https://cdn.discordapp.com/avatars/{user.Id}/{user.Avatar}.png";
    }
}