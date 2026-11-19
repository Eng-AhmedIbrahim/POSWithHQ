using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace HQ.Desktop.Auth;

public sealed class HqAuthStateProvider : AuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());
    public List<string> UserPermissions { get; private set; } = new();
    public string Token { get; private set; } = string.Empty;
    public string CurrentUserName => _currentUser.Identity?.Name ?? string.Empty;
    public bool IsAuthenticated => _currentUser.Identity?.IsAuthenticated ?? false;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(_currentUser));

    public void SignIn(string token, string displayName, List<string>? permissions = null)
    {
        Token = token;
        UserPermissions = permissions ?? new List<string>();

        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                _currentUser = ParseClaimsFromJwt(token, displayName);
                // Also extract permissions from JWT claims if available
                var tokenPerms = _currentUser.Claims
                    .Where(c => c.Type == "Permission" || c.Type == "permission")
                    .Select(c => c.Value)
                    .ToList();
                foreach (var p in tokenPerms)
                {
                    if (!UserPermissions.Contains(p))
                        UserPermissions.Add(p);
                }
            }
            catch
            {
                var identity = new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Name, displayName),
                        new Claim(ClaimTypes.Role, "User")
                    ],
                    authenticationType: "JwtAuth");
                _currentUser = new ClaimsPrincipal(identity);
            }
        }
        else
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.Role, "User")
                ],
                authenticationType: "HqPin");
            _currentUser = new ClaimsPrincipal(identity);
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public void SignOut()
    {
        Token = string.Empty;
        UserPermissions.Clear();
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public bool HasPermission(string permissionName)
    {
        if (_currentUser.IsInRole("Administrator") || _currentUser.HasClaim(ClaimTypes.Role, "Administrator"))
            return true;

        return UserPermissions.Contains(permissionName, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyHqPermission()
    {
        if (_currentUser.IsInRole("Administrator") || _currentUser.HasClaim(ClaimTypes.Role, "Administrator"))
            return true;

        return UserPermissions.Any(p => p.StartsWith("CanAccessHQ", StringComparison.OrdinalIgnoreCase)
                                     || p.StartsWith("CanViewHQ", StringComparison.OrdinalIgnoreCase)
                                     || p.StartsWith("CanExportHQ", StringComparison.OrdinalIgnoreCase)
                                     || p.StartsWith("CanManageHQ", StringComparison.OrdinalIgnoreCase));
    }

    private static ClaimsPrincipal ParseClaimsFromJwt(string token, string fallbackDisplayName)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims.ToList();

        if (!claims.Any(c => c.Type == ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, fallbackDisplayName));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new ClaimsPrincipal(identity);
    }
}
