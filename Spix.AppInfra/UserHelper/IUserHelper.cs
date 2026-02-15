using Microsoft.AspNetCore.Identity;
using Spix.Domain.Entities;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EnumTypes;

namespace Spix.AppInfra.UserHelper;

public interface IUserHelper
{
    // ============================================================
    // USERS
    // ============================================================
    Task<User> GetUserByUserNameAsync(string userName);
    Task<User> GetUserByEmailAsync(string email);
    Task<User> GetUserByIdAsync(Guid userId);
    Task<IdentityResult> AddUserAsync(User user, string password);
    Task<bool> DeleteUser(string username);
    Task<IdentityResult> UpdateUserAsync(User user);

    // ============================================================
    // ROLES
    // ============================================================
    Task CheckRoleAsync(string roleName);
    Task AddUserToRoleAsync(User user, string roleName);
    Task RemoveUserToRoleAsync(User user, string roleName);
    Task<bool> IsUserInRoleAsync(User user, string roleName);

    // ============================================================
    // CLAIMS
    // ============================================================
    Task AddUserClaims(UserType userType, string userName);
    Task RemoveUserClaims(UserType userType, string userName);

    // ============================================================
    // LOGIN / LOGOUT
    // ============================================================
    Task<SignInResult> LoginAsync(LoginDTO model);
    Task LogoutAsync();
    Task<SignInResult> ValidatePasswordAsync(User user, string password);

    // ============================================================
    // PASSWORDS
    // ============================================================
    Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

    // ============================================================
    // EMAIL CONFIRMATION
    // ============================================================
    Task<string> GenerateEmailConfirmationTokenAsync(User user);
    Task<IdentityResult> ConfirmEmailAsync(User user, string token);

    // ============================================================
    // PASSWORD RESET
    // ============================================================
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

    // ============================================================
    // CUSTOM USER CREATION (TU LÓGICA)
    // ============================================================
    Task<User> AddUserUsuarioAsync(string firstname, string lastname, string username,
                    string email, string phone, string address, string job,
                    int Idcorporate, string ImagenFile, string Origin, bool UserActivo, UserType usertype);

    Task<User> AddUserUsuarioSoftAsync(string firstname, string lastname, string username,
                    string email, string phone, string address, string job,
                    int Idcorporate, string ImagenFile, string Origin, bool UserActivo);

}