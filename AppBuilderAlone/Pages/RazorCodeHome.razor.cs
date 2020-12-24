using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Client.Components;
using AppBuilder.Client.Components.RazorProject;
using AppBuilder.Client.ExtensionMethods;
using AppBuilder.Client.Services;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

//using Newtonsoft.Json;

namespace AppBuilder.Client.Pages
{
    public partial class RazorCodeHome : IDisposable
    {       
        [Inject]
        public AppState AppState { get; set; }
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        [Inject]
        public RazorCompile RazorCompile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        private RazorInterop RazorInterop => new(JsRuntime);
        public List<ProjectFile> Files { get; set; } = new();
        private DotNetObjectReference<RazorCodeHome> dotNetInstance;
        private string language = "razor";
        private bool isready;
        private static string sampleSnippet = CodeSnippets.RazorSnippet;

        private List<string> Diagnostics { get; set; } = new();
        private bool isCodeCompiling;
        private bool isCSharp;
        private string buttonCss = "";
        private string iframeSrc;
        protected override async Task OnInitializedAsync()
        {
            //AppState.CodeSnippet = sampleSnippet;
            iframeSrc = NavigationManager.BaseUri.ToString() + "/defaultOutput";
            await Task.Delay(50);
            await RazorCompile.InitAsync();
            AppState.PropertyChanged += HandleCodePropertyChanged;
            isready = true;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var mainCodeFile = new ProjectFile { Name = RazorConstants.DefaultComponentName, Content = sampleSnippet, FileType = FileType.Razor };
                AppState.ActiveProjectFile = mainCodeFile;

                AppState.ActiveProject ??= new UserProject { Name = "DefaultProject", Files = new List<ProjectFile> { mainCodeFile } };
                //AppState.CodeSnippet = sampleSnippet;
                dotNetInstance = DotNetObjectReference.Create(this);
                await RazorInterop.RazorAppInit(dotNetInstance);
            }
            Console.WriteLine("RazorCodeHome Renders");
            await base.OnAfterRenderAsync(firstRender);
        }

        private void HandleSaveToProject(string content)
        {
            AppState.ActiveProjectFile.Content = content;
            var currentFile = AppState.ActiveProjectFile;
            currentFile.Content = content;
            AppState.SaveCode(currentFile);
            if (!AppState.HasActiveProject) return;

            AppState.ActiveProject.Files = AppState.ProjectFiles;
        }

        private void StartExecute()
        {
            isCodeCompiling = true;
            StateHasChanged();
            _ = ExecuteProject();
        }
        protected async Task ExecuteProject()
        {
            await Task.Delay(50);
            Diagnostics = new List<string>();
            CodeAssemblyModel compilationResult = null;
            ProjectFile mainComponent = null;
            string originalMainComponentContent = null;
            originalMainComponentContent = AppState.ProjectFiles
                .FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName)
                ?.Content ?? RazorConstants.DefaultFileContent;
            var codeFiles = AppState.ProjectFiles.PagifyMainComponent();

            compilationResult = await RazorCompile.CompileToAssemblyAsync(codeFiles);
            Diagnostics.AddRange(compilationResult?.Diagnostics?.Select(x => x.ToString()) ?? new List<string> { "None" });
            buttonCss = Diagnostics.Any() ? "alert_output" : "";
            if (compilationResult?.AssemblyBytes?.Length > 0)
                await RazorInterop.RazorCacheAndDisplay(compilationResult.AssemblyBytes);

            isCodeCompiling = false;
            AppState.ProjectFiles = codeFiles.UnPagifyMainComponent(originalMainComponentContent);
            await InvokeAsync(StateHasChanged);
        }
       
        private void HandleTabFileChange(BaseMatTabLabel baseMatTab)
        {
            AppState.ActiveProjectFile = AppState.ProjectFiles.FirstOrDefault(x => x.Name == baseMatTab.Id);
        }
        private void HandleCodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName !=nameof(AppState.ActiveProjectFile) && args.PropertyName != nameof(AppState.ActiveProject)) return;
            //AppState.CodeSnippet = AppState.ActiveProjectFile.Content;
            AppState.ProjectFiles = AppState.ActiveProject.Files;
            StateHasChanged();
        }
        
        private async Task ShowDiags()
        {
            buttonCss = "";
            var asMarkups = Diagnostics.Select(diagnostic => $"<li>{diagnostic}</li>");
            var content = $"<ol>{string.Join(' ', asMarkups)}</ol>";
            var parameters = new ModalDialogParameters
            {
                {"CodeOutput", content}
            };
            await ModalService.ShowDialogAsync<CodeOutModal>("Current diagnostics are displayed here.",
                parameters: parameters);
        }
        [JSInvokable("ShowCacheError")]
        public async void ShowCacheError() =>
            await ModalService.ShowMessageBoxAsync("Project Not Found",
                "Hmm... It appears that the project is longer in your browser cache. I blame you, but perhaps refreshing the app and trying again will resolve it.");
        private async Task ShowMenu()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-appMenu",
            };
            var result = await ModalService.ShowDialogAsync<AppMenu>("Action Menu", option);
        }
        public void Dispose()
        {
            dotNetInstance?.Dispose();
            AppState.PropertyChanged -= HandleCodePropertyChanged;
            Console.WriteLine("RazorCodeHome.razor disposed");
        }
    }
}
