using Microsoft.AspNetCore.Identity;
using Spix.AppInfra.UtilityTools;
using Spix.Domain.Entities;
using Spix.DomainLogic.AppResponses;
using Spix.DomainLogic.EnumTypes;
using System.Security.Claims;

namespace Spix.AppInfra.UserHelper;

public class UserHelper : IUserHelper
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUtilityTools _utilityTools;

    public UserHelper(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<User> signInManager,
        IUtilityTools utilityTools)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _utilityTools = utilityTools;
    }

    // ============================================================
    // USERS
    // ============================================================

    public async Task<User> GetUserByUserNameAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user!;
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user!;
    }


    public async Task<User> GetUserByIdAsync(Guid userId)
    {
        string id = userId.ToString();
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user!;
    }


    public async Task<IdentityResult> AddUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result;
    }

    public async Task<bool> DeleteUser(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        User? user = await _userManager.FindByNameAsync(username);
        if (user == null) return false;

        IdentityResult result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        return await _userManager.UpdateAsync(user);
    }

    // ============================================================
    // ROLES
    // ============================================================

    public async Task CheckRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole(roleName));
    }

    public async Task AddUserToRoleAsync(User user, string roleName)
        => await _userManager.AddToRoleAsync(user, roleName);

    public async Task RemoveUserToRoleAsync(User user, string roleName)
        => await _userManager.RemoveFromRoleAsync(user, roleName);

    public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        => await _userManager.IsInRoleAsync(user, roleName);

    // ============================================================
    // CLAIMS
    // ============================================================

    public async Task AddUserClaims(UserType userType, string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
            await _userManager.AddClaimAsync(user, new Claim(userType.ToString(), "1"));
    }

    public async Task RemoveUserClaims(UserType userType, string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user != null)
            await _userManager.RemoveClaimAsync(user, new Claim(userType.ToString(), "1"));
    }

    // ============================================================
    // LOGIN / LOGOUT
    // ============================================================

    public async Task<SignInResult> LoginAsync(LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.UserName);

        if (user == null || !user.Active)
            return SignInResult.Failed;

        return await _signInManager.PasswordSignInAsync(
            model.UserName,
            model.Password,
            isPersistent: false,
            lockoutOnFailure: false
        );
    }

    public async Task LogoutAsync()
        => await _signInManager.SignOutAsync();

    public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        => await _signInManager.CheckPasswordSignInAsync(user, password, false);

    // ============================================================
    // PASSWORDS
    // ============================================================

    public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        => await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);

    // ============================================================
    // EMAIL CONFIRMATION
    // ============================================================

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        => await _userManager.GenerateEmailConfirmationTokenAsync(user);

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        => await _userManager.ConfirmEmailAsync(user, token);

    // ============================================================
    // PASSWORD RESET
    // ============================================================

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
        => await _userManager.GeneratePasswordResetTokenAsync(user);

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        => await _userManager.ResetPasswordAsync(user, token, password);

    // ============================================================
    // CUSTOM USER CREATION (TU LÓGICA)
    // ============================================================

    public async Task<User> AddUserUsuarioAsync(string firstname, string lastname, string username,
            string email, string phone, string address, string job,
            int Idcorporate, string ImagenFull, string Origin, bool UserActivo, UserType usertype)
    {
        var clave = _utilityTools.GeneratePass(8);

        var user = new User
        {
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            UserName = username,
            PhoneNumber = phone,
            CorporationId = Idcorporate,
            PhotoUser = ImagenFull,
            JobPosition = job,
            UserFrom = Origin,
            Active = UserActivo,
            UserRoleDetails = new List<UserRoleDetails>
            {
                new UserRoleDetails { UserType = usertype }
            },
            Pass = clave
        };

        var result = await _userManager.CreateAsync(user, clave);
        if (!result.Succeeded)
            return null!;

        var newUser = await GetUserByUserNameAsync(username);

        if (newUser != null)
        {
            await AddUserToRoleAsync(newUser, usertype.ToString());
            await AddUserClaims(usertype, username);
        }

        return newUser!;
    }

    public async Task<User> AddUserUsuarioSoftAsync(string firstname, string lastname, string username,
    string email, string phone, string address, string job,
    int Idcorporate, string ImagenFull, string Origin, bool UserActivo)
    {
        var clave = _utilityTools.GeneratePass(8);

        var user = new User
        {
            FirstName = firstname,
            LastName = lastname,
            Email = email,
            UserName = username,
            PhoneNumber = phone,
            CorporationId = Idcorporate,
            PhotoUser = ImagenFull,
            JobPosition = job,
            UserFrom = Origin,
            Active = UserActivo,
            Pass = clave
        };

        var result = await _userManager.CreateAsync(user, clave);
        if (!result.Succeeded)
            return null!;

        return await GetUserByUserNameAsync(username);
    }

}