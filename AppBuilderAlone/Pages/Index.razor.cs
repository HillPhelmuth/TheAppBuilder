using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Client.Components;
using AppBuilder.Client.Services;
using AppBuilder.Client.StaticCustomAuth.Interfaces;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.JSInterop;

namespace AppBuilder.Client.Pages
{
    public partial class Index : IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IModalDialogService ModalService { get; set; }
        [Inject]
        private ConsoleCompile CompileService { get; set; }
        [Inject]
        private RazorCompile RazorCompile { get; set; }
        [Inject]
        private IJSRuntime JsRuntime { get; set; }
        [Inject]
        private ISyncLocalStorageService LocalStorage { get; set; }
        [Inject]
        private ICustomAuthenticationStateProvider AuthenticationState { get; set; }
        private RazorInterop RazorInterop => new(JsRuntime);
        private List<ProjectFile> ExtractedFiles { get; set; } = new();
        private DotNetObjectReference<Index> IndexObject;
        private string projectName;

        private async Task SetAuthStateValues()
        {
            var authState = await AuthenticationState.GetAuthenticationStateAsync();
           AppState.IsAuthUser = authState.User?.Identity?.IsAuthenticated ?? false;
           AppState.CurrentUser = AppState.IsAuthUser ? authState.User?.Identity?.Name : "none";
        }

        protected override async Task OnInitializedAsync()
        {
            await SetAuthStateValues();
            AppState.PropertyChanged += HandleCodePropertyChanged;
            await base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await RazorCompile.InitAsync();
                //await CompileService.InitAsync();
                IndexObject = DotNetObjectReference.Create(this);
                AppState.IsOnline = await RazorInterop.CheckOnlineStatus();
                await RazorInterop.TrackOnlineStatus(IndexObject);
            }
            Console.WriteLine("Index Renders");
            await base.OnAfterRenderAsync(firstRender);
        }

        private void HandleProjectUpload(UserProject project)
        {
            if (project?.Files == null) return;
            AppState.ActiveProject = project;
            AppState.ProjectFiles = project.Files;
        }
      
        private void RecoverState()
        {
            var storedState = LocalStorage.GetItem<AppState>($"{AppState.CurrentUser}_{nameof(AppState)}");
            AppState.SetStateFromStorage(storedState);
        }
        [JSInvokable("HandleOnOffLine")]
        public void HandleOnOffLine(string status)
        {
            AppState.IsOnline = status == "online";
            if (!AppState.IsOnline)
                LocalStorage.SetItem($"{AppState.CurrentUser}_{nameof(AppState)}", AppState);
            
            Console.WriteLine($"network status changed to {status}");
        }
        private void HandleCodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (AppState.IsAuthUser) LocalStorage.SetItem(nameof(AppState), AppState);
            if (args.PropertyName != "ProjectFiles" && args.PropertyName != "ActiveProject") return;
            ExtractedFiles = AppState.ProjectFiles;
            StateHasChanged();
        }
        
        public void Dispose()
        {
            IndexObject.Dispose();
            AppState.PropertyChanged -= HandleCodePropertyChanged;
        }
    }
}
