using System;
using System.Collections.Generic;

namespace SharedComponents.PostgreSQL;

/// <summary>
/// Table containing user data
/// </summary>
public partial class UserCredential
{
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string PasswordSalt { get; set; } = null!;

    /// <summary>
    /// true if email verification process was succesful
    /// </summary>
    public bool EmailIsVerified { get; set; }

    /// <summary>
    /// token to be sent via email in case of verification or password reset
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    public string Role { get; set; } = null!;

    /// <summary>
    /// temporary token for password reset request
    /// </summary>
    public string? PasswordResetToken { get; set; }
}
