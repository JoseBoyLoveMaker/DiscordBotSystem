using System.Collections.Concurrent;

public class UserSessionStore
{
    private readonly ConcurrentDictionary<string, AuthenticatedDiscordUser> _sessions = new();

    public void Set(string sessionId, AuthenticatedDiscordUser user)
    {
        _sessions[sessionId] = user;
    }

    public AuthenticatedDiscordUser? Get(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var user))
        {
            if (user.ExpiresAtUtc > DateTime.UtcNow)
                return user;

            _sessions.TryRemove(sessionId, out _);
        }

        return null;
    }

    public void Remove(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }
}