using System;
using System.Net.Http;
using System.Threading.Tasks;
using AppBuilder.Client.StaticCustomAuth;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using MatBlazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AppBuilder.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<GithubClient>();
            builder.Services.AddMatBlazor();
            builder.Services.AddModalDialog();
            builder.Services.AddSharedServices();
            builder.Services.AddConsoleServices();
            builder.Services.AddAuthentication();
            builder.Services.AddScoped<RazorCompile>();
            await builder.Build().RunAsync();
        }
    }
}
