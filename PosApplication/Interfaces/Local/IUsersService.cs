using System.Collections.Generic;
using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Local
{
    public interface IUsersService
    {
        Task<IReadOnlyList<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> UsernameExistsAsync(string username, int? excludingUserId = null);
        Task<User> CreateUserAsync(string username, string pin, string role);
        Task UpdateUserAsync(User user, string username, string pin, string role);
        Task ResetPinAsync(int userId, string newPin);
        Task DeleteUserAsync(int id);
    }
}
