using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using AppBuilder.Client.StaticCustomAuth.Interfaces;
using AppBuilder.Client.StaticCustomAuth.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;

namespace AppBuilder.Client.StaticCustomAuth
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider, ICustomAuthenticationStateProvider
    {
        private readonly IConfiguration _config;
        private readonly HttpClient http;

        public CustomAuthenticationStateProvider(IConfiguration config, IWebAssemblyHostEnvironment environment)
        {
            _config = config;
            http = new HttpClient { BaseAddress = new Uri(environment.BaseAddress) };
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var authDataUrl = _config.GetValue<string>("StaticWebAppsAuthentication:AuthenticationDataUrl", "/.auth/me");
                var data = await http.GetFromJsonAsync<AuthenticationData>(authDataUrl);

                var principal = data.ClientPrincipal;
                principal.UserRoles = principal.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

                var principalUserRoles = principal.UserRoles.ToList();
                if (!principalUserRoles.Any())
                {
                    return new AuthenticationState(new ClaimsPrincipal());
                }

                var identity = new ClaimsIdentity(principal.IdentityProvider);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
                identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
                identity.AddClaims(principalUserRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal());
            }
        }
    }
}