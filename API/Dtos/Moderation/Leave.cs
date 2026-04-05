public class UpdateLeaveConfigDto
{
    public bool Enabled { get; set; }
    public ulong ChannelId { get; set; }
    public string Message { get; set; } = string.Empty;
}