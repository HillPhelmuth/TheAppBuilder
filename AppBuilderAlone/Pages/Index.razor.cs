using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Client.Components;
using AppBuilder.Client.Services;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using AppBuilder.Shared.StaticAuth.Interfaces;
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
        private GithubClient GithubClient { get; set; }
        [Inject]
        private ZipService ZipService { get; set; }
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
        private string orgName;
        private string repoName;
        private string fileName;

        protected override async Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleCodePropertyChanged;
            await base.OnInitializedAsync();
        }
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await CompileService.InitAsync();
                await RazorCompile.InitAsync();
                IndexObject = DotNetObjectReference.Create(this);
                await RazorInterop.InitOnOffLine(IndexObject);
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private void HandleProjectUpload(List<ProjectFile> projectFiles)
        {
            AppState.ProjectFiles = projectFiles;
            ExtractedFiles = projectFiles;
        }
        private async Task ShowMenu()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-appMenu",
            };
            var result = await ModalService.ShowDialogAsync<AppMenu>("Action Menu", option);
            if (!result.Success) return;
        }
        private async Task DownloadRepo()
        {
            var inputForm = new ModalDataInputForm("Download Repo", "Provide the GitHub organization and repo name");
            var orgField = inputForm.AddStringField("Org", "Org Name", "");
            var repoField = inputForm.AddStringField("Repo", "Repo Name", "");
            var filepathField = inputForm.AddStringField("File", "File path", "");

            if (!await inputForm.ShowAsync(ModalService))
                return;
            orgName = orgField.Value;
            repoName = repoField.Value;
            fileName = filepathField.Value;
            var apiResponse = await GithubClient.CodeFromPublicRepo(orgName, repoName, fileName);
            ExtractedFiles = await ZipService.ExtractFiles(apiResponse);
        }

        private void RecoverState()
        {
            var storedState = LocalStorage.GetItem<AppState>(nameof(AppState));
            AppState.SetStateFromStorage(storedState);
        }
        [JSInvokable("HandleOnOffLine")]
        public void HandleOnOffLine(string status)
        {
            AppState.IsOnline = status == "online";
            Console.WriteLine($"network status changed to {status}");
        }
        private void HandleCodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (AppState.IsAuthUser) LocalStorage.SetItem($"{AppState.CurrentUser}_{nameof(AppState)}", AppState);
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
