using PosDomain.Interfaces;
using System.Threading.Tasks;
using PosDomain.Entities;

namespace PosApplication.Interfaces.Server
{
    public interface IAuthService
    {
        Task<(bool IsSuccess, string Message, User? User, string? Token, string? RefreshToken)> LoginAsync(LoginRequest request);
        Task<(bool IsSuccess, string Message, string? Token, string? RefreshToken)> RefreshAsync(TokenRequest request);
        Task<(bool IsSuccess, string Message, string LicenseKey)> ProvisionAsync(ProvisionRequest request);
    }
}
