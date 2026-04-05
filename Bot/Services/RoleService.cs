using Discord.WebSocket;

async Task CheckLevelRoles(SocketGuildUser user, GuildUserData data, GuildConfig config)
{
    if (config.Roles?.LevelRoles == null || config.Roles.LevelRoles.Count == 0)
        return;

    foreach (var reward in config.Roles.LevelRoles)
    {
        if (data.ChatLevel >= reward.MinChatLevel &&
            data.CallLevel >= reward.MinCallLevel)
        {
            var role = user.Guild.GetRole(reward.RoleId);

            if (role == null)
                continue;

            if (!user.Roles.Contains(role))
            {
                await user.AddRoleAsync(role);
            }
        }
    }
}