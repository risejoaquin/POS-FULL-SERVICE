using Microsoft.EntityFrameworkCore;
using PosApplication.Interfaces.Local;
using PosDomain.Entities;
using PosInfrastructure.Data.Local;

namespace PosInfrastructure.Services.Local;

public class UsersService : IUsersService
{
    private readonly PosDbContext _dbContext;

    public UsersService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> UsernameExistsAsync(string username, int? excludingUserId = null)
    {
        var normalizedUsername = NormalizeUsername(username);
        return await _dbContext.Users.AnyAsync(u =>
            u.Username != null &&
            u.Username.ToLower() == normalizedUsername &&
            (!excludingUserId.HasValue || u.Id != excludingUserId.Value));
    }

    public async Task<User> CreateUserAsync(string username, string pin, string role)
    {
        var user = new User
        {
            Username = username.Trim(),
            Pin = null,
            PasswordHash = HashCredential(pin),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task UpdateUserAsync(User user, string username, string pin, string role)
    {
        var existingUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existingUser == null)
        {
            return;
        }

        existingUser.Username = username.Trim();
        existingUser.Pin = null;
        existingUser.PasswordHash = HashCredential(pin);
        existingUser.Role = role;
        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetPinAsync(int userId, string newPin)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return;
        }

        user.Pin = null;
        user.PasswordHash = HashCredential(newPin);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim().ToLowerInvariant();
    }

    private static string HashCredential(string credential)
    {
        return BCrypt.Net.BCrypt.HashPassword(credential);
    }
}
