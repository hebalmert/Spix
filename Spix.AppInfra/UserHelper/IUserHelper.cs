using Microsoft.AspNetCore.Identity;
using Spix.Domain.Entities;
using Spix.Domain.Enum;
using Spix.DomainLogic.ResponcesSec;

namespace Spix.AppInfra.UserHelper;

public interface IUserHelper
{
    Task<User> GetUserByUserNameAsync(string userName);

    Task<User> GetUserByEmailAsync(string email);

    Task<User> GetUserByIdAsync(Guid userId);

    Task<User> GetUserAsync(string email);

    Task<IdentityResult> AddUserAsync(User user, string password);

    Task<bool> DeleteUser(string username);

    Task CheckRoleAsync(string roleName);

    Task AddUserToRoleAsync(User user, string roleName);

    Task RemoveUserToRoleAsync(User user, string roleName);

    Task AddUserClaims(UserType userType, string userName);

    Task RemoveUserClaims(UserType userType, string userName);

    Task<bool> IsUserInRoleAsync(User user, string roleName);

    Task<SignInResult> LoginAsync(LoginDTO model);

    Task LogoutAsync();

    //Para Administrar El Cambio de Clave de los usuarios
    Task<IdentityResult> UpdateUserAsync(User user);

    Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword);

    //Metodos para Validar el Correo del Usuario para activar la cuenta

    Task<string> GenerateEmailConfirmationTokenAsync(User user);

    Task<IdentityResult> ConfirmEmailAsync(User user, string token);

    //Recuperacion de la Clave de Usuario
    Task<string> GeneratePasswordResetTokenAsync(User user);

    Task<IdentityResult> ResetPasswordAsync(User user, string token, string password);

    //Registro de Usuarios al Sistema de User
    //Se pasa el Usuario y el Pass, para ver si es Valido el Usuario, nos da un Result
    //metodo para implementar la socitud del token de seguridad.
    Task<SignInResult> ValidatePasswordAsync(User user, string password);

    //Para Registro de UserASP desde el modulo de Usuarios / Sistema de Corporacion
    Task<User> AddUserUsuarioAsync(string firstname, string lastname, string username,
                    string email, string phone, string address, string job,
                    int Idcorporate, string ImagenFile, string Origin, bool UserActivo, UserType usertype);

    Task<User> AddUserUsuarioSoftAsync(string firstname, string lastname, string username,
                    string email, string phone, string address, string job,
                    int Idcorporate, string ImagenFile, string Origin, bool UserActivo);
}