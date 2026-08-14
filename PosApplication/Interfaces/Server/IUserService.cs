using PosDomain.Interfaces;
using PosDomain.Entities;
using System.Threading.Tasks;

namespace PosApplication.Interfaces.Server
{
    public interface IUserService
    {
        Task<(bool isSuccess, string message, User? user)> CreateOrUpdateUserAsync(User? user);
        Task<bool> DeleteUserAsync(string username);
    }
}
