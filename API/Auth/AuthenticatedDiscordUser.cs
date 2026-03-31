public class AuthenticatedDiscordUser
{
    public string DiscordId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? GlobalName { get; set; }
    public string? Avatar { get; set; }
    public string? AvatarUrl { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}