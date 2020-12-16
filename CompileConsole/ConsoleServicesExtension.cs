using AppBuilder.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace AppBuilder.CompileConsole
{
    public static class ConsoleServicesExtension
    {
        public static IServiceCollection AddConsoleServices(this IServiceCollection service)
        {
            //service.AddScoped<IDependencyResolver, ConsoleDependencyResolver>();
            service.AddScoped<ConsoleCompile>();
            
            return service;
        }
    }
}
