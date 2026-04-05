public class UpdateWelcomeConfigDto
{
    public bool Enabled { get; set; }
    public ulong ChannelId { get; set; }
    public string Message { get; set; } = string.Empty;
    public ulong AutoRoleId { get; set; }
}