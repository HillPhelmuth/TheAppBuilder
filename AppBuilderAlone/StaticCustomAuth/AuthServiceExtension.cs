using AppBuilder.Client.StaticCustomAuth.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AppBuilder.Client.StaticCustomAuth
{
    public static class AuthServiceExtension
    {
        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            return services
                .AddAuthorizationCore()
                .AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>()
                .AddScoped<ICustomAuthenticationStateProvider, CustomAuthenticationStateProvider>();
        }
    }
}