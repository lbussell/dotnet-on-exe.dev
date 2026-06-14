namespace TodoApp.Authentication;

/// <summary>
/// Constants describing how exe.dev's HTTP proxy communicates the authenticated
/// user to the application. See https://exe.dev/docs/login-with-exe.md.
/// </summary>
public static class ExeDevAuthentication
{
    /// <summary>Header carrying a stable, unique user identifier.</summary>
    public const string UserIdHeader = "X-ExeDev-UserID";

    /// <summary>Header carrying the user's email address.</summary>
    public const string EmailHeader = "X-ExeDev-Email";
}
