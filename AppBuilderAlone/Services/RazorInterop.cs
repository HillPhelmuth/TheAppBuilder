using System;
using System.Threading.Tasks;
using AppBuilder.Client.Pages;
using AppBuilder.CompileRazor;
using Microsoft.JSInterop;
using Index = AppBuilder.Client.Pages.Index;

namespace AppBuilder.Client.Services
{
    public class RazorInterop : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> moduleTask;

        public RazorInterop(IJSRuntime jsRuntime)
        {
            moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/razorApp.js").AsTask());
        }

        public async ValueTask RazorAppInit(DotNetObjectReference<RazorCodeHome> dotNetInstance)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("init", dotNetInstance);
        }

        public async ValueTask RazorCacheAndDisplay(byte[] assemblyBytes)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("updateUserAssemblyInCacheStorage", assemblyBytes);

            await module.InvokeVoidAsync("reloadIFrame", "user-page-window", RazorConstants.MainComponentPagePath);
        }
        public async ValueTask DownloadProjectFile(string filename, byte[] projectBytes)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("saveAsFile", filename, projectBytes);
        }

        public async ValueTask InitOnOffLine(DotNetObjectReference<Index> objRef)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("initOnOffLine", objRef);
        }
        public async ValueTask CopyToClipboard(string text)
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("copyToClipboard", text);
        }
        public async ValueTask DisposeAsync()
        {
            if (moduleTask.IsValueCreated)
            {
                var module = await moduleTask.Value;
                await module.InvokeVoidAsync("dispose");
                await module.DisposeAsync();
            }
        }
    }
}

