using Microsoft.Extensions.DependencyInjection;

namespace Shared
{
    public static class SharedServiceExtension
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection service)
        {
            service.AddScoped<ZipService>();
            service.AddSingleton<AppState>();
            return service;
        }
    }
}
