using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
//using AppBuilderAlone.Pages;
using CompileRazor;
using Microsoft.JSInterop;

namespace AppBuilderAlone.Services
{
    public class RazorInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public RazorInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/AppBuilderAlone/razorApp.js").AsTask());
        }

        public async ValueTask RazorAppInit(DotNetObjectReference<RazorCodeHome> dotNetInstance)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("init", dotNetInstance);
        }

        public async ValueTask RazorCacheAndDisplay(byte[] assemblyBytes)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("App.Razor.updateUserAssemblyInCacheStorage", assemblyBytes);

            await module.InvokeVoidAsync("App.reloadIFrame", "user-page-window", RazorConstants.MainComponentPagePath);
        }
        public async ValueTask DisposeAsync()
        {

        }
    }
}

