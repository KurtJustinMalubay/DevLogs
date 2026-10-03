using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace DevLogs.Services;

public record UserAccount(string Username, string Email, byte[] Salt, byte[] Hash);

/// <summary>
/// In-memory accounts with PBKDF2-hashed passwords.
/// </summary>
public class AuthService
{
    static readonly ConcurrentDictionary<string, UserAccount> Users = new(StringComparer.OrdinalIgnoreCase);

    public UserAccount? Current { get; private set; }
    public bool IsAuthenticated => Current is not null;

    static byte[] Hash(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);

    public async Task<(bool Ok, string? Error)> RegisterAsync(string username, string email, string password)
    {
        await Task.Delay(250);
        if (Users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (false, "That email is already registered. Try signing in instead.");
        var salt = RandomNumberGenerator.GetBytes(16);
        var account = new UserAccount(username, email, salt, Hash(password, salt));
        if (!Users.TryAdd(username, account))
            return (false, "That username is taken. Pick another one.");
        Current = account;
        return (true, null);
    }

    public async Task<(bool Ok, string? Error)> LoginAsync(string id, string password)
    {
        await Task.Delay(250);
        var account = Users.Values.FirstOrDefault(u =>
            u.Username.Equals(id, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (account is null || !CryptographicOperations.FixedTimeEquals(account.Hash, Hash(password, account.Salt)))
            return (false, "Username/email or password is incorrect.");
        Current = account;
        return (true, null);
    }

    public void SignOut() => Current = null;

    public async Task<(bool Ok, string? Error)> UpdateProfileAsync(string username, string email)
    {
        await Task.Delay(250);
        if (Current == null) return (false, "Not logged in.");
        
        if (!Current.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && 
            Users.Values.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            return (false, "Email is already taken.");
            
        if (!Current.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
        {
            if (Users.ContainsKey(username)) return (false, "Username is already taken.");
            Users.TryRemove(Current.Username, out _);
        }
        
        var updated = Current with { Username = username, Email = email };
        Users[username] = updated;
        Current = updated;
        return (true, null);
    }
}
