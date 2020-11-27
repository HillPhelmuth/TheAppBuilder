using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CompileConsole
{
    public static class ConsoleServicesExtension
    {

        public static IServiceCollection AddConsoleServices(this IServiceCollection service)
        {
            service.AddScoped<IDependencyResolver, BlazorDependencyResolver>();
            service.AddScoped<CSharpCompile>();
            return service;
        }
    }
}
