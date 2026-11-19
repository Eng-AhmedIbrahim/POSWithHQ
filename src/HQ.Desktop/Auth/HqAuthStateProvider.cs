using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace HQ.Desktop.Auth;

public sealed class HqAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _user = new(new ClaimsIdentity());

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_user));

    public void SignIn(string displayName)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, displayName)],
            authenticationType: "HqPin");
        _user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        _user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
