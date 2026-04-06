using System.Text.Json.Serialization;

public class ModerationDto
{
    [JsonPropertyName("welcomeConfig")]
    public WelcomeConfig WelcomeConfig { get; set; } = new();

    [JsonPropertyName("leaveConfig")]
    public LeaveConfig LeaveConfig { get; set; } = new();

    [JsonPropertyName("roleConfig")]
    public RoleConfig RoleConfig { get; set; } = new();
}