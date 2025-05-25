using Microsoft.AspNetCore.Identity;

namespace AuthService.Services;

public class PasswordVerificationService
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public PasswordVerificationService(IPasswordHasher<object> passwordHasher)
    {
        _passwordHasher = passwordHasher;
    }

    public bool VerifyPassword(string storedHashedPassword, string providedPassword)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            new object(),
            storedHashedPassword,
            providedPassword);

        switch (verificationResult)
        {
            case PasswordVerificationResult.Success:
            case PasswordVerificationResult.SuccessRehashNeeded:
                return true;
            default:
                return false;
        }
    }
}
