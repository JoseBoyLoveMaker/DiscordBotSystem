using System.Text.Json.Serialization;

public class DiscordGuildDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("owner")]
    public bool Owner { get; set; }

    [JsonPropertyName("permissions")]
    public ulong Permissions { get; set; }
}