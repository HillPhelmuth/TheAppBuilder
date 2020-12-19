
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace AppBuilder.Shared
{
    public static class SharedServiceExtension
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection service)
        {
            service.AddBlazoredLocalStorage();
            service.AddScoped<IDependencyResolver, DependencyResolver>();
            service.AddScoped<ZipService>();
            service.AddSingleton<AppState>();
            return service;
        }
    }
}
