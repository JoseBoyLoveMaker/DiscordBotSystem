public class UpdateRoleConfigDto
{
    public List<LevelRoleRewardDto> LevelRoles { get; set; } = new();
}

public class LevelRoleRewardDto
{
    public ulong RoleId { get; set; }
    public int MinChatLevel { get; set; }
    public int MinCallLevel { get; set; }
}