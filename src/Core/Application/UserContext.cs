using EcoBank.Core.Domain.Auth;
using EcoBank.Core.Domain.Users;

namespace EcoBank.Core.Application;

public class UserContext
{
    public Credentials? Credentials { get; private set; }
    public TokenResponse? Token { get; private set; }
    public User? SelectedUser { get; private set; }

    public bool IsAuthenticated => Token is not null && Token.ExpiresAt > DateTimeOffset.UtcNow;
    public bool HasSelectedUser => SelectedUser is not null;

    public void SetCredentials(Credentials credentials) => Credentials = credentials;
    public void SetToken(TokenResponse token) => Token = token;
    public void SelectUser(User user) => SelectedUser = user;

    public void Reset()
    {
        Credentials = null;
        Token = null;
        SelectedUser = null;
    }
}
