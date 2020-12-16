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
        private RazorInterop RazorInterop => new RazorInterop(JsRuntime);

        //private const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        //private const string MainUserPagePath = "/__razorOutput";
        public const string MainComponentFilePath = "__RazorOutput.razor";
        public List<ProjectFile> Files { get; set; } = new List<ProjectFile>();
        private DotNetObjectReference<RazorCodeHome> dotNetInstance;
        private string language = "razor";
        private bool isready;
        private static string sampleSnippet = CodeSnippets.RazorSnippet;

        private List<string> Diagnostics { get; set; } = new List<string>();
        private bool isCodeCompiling;
        private bool isCSharp;
        private string buttonCss = "";
        protected override async Task OnInitializedAsync()
        {
            //AppState.CodeSnippet = sampleSnippet;
            var mainCodeFile = new ProjectFile { Name = MainComponentFilePath, Content = sampleSnippet, FileType = FileType.Razor };
            AppState.ActiveProjectFile = mainCodeFile;
           
            AppState.ActiveProject ??= new UserProject { Name = "DefaultProject", Files = new List<ProjectFile> { mainCodeFile } };
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
                AppState.ActiveProjectFile ??= new ProjectFile { Name = MainComponentFilePath, Content = sampleSnippet, FileType = FileType.Razor };
                //AppState.CodeSnippet = sampleSnippet;
                dotNetInstance = DotNetObjectReference.Create(this);
                await RazorInterop.RazorAppInit(dotNetInstance);
            }
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
        protected async Task UpdateFromPublicRepo()
        {
            var option = new ModalDialogOptions
            {
                Style = "modal-dialog-githubform"
            };
            var result = await ModalService.ShowDialogAsync<GitHubForm>("Get code from a public Github Repo", option);
            if (!result.Success) return;
            string code = result.ReturnParameters.Get<string>("FileCode");
            AppState.CodeSnippet = code;
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

        public void Dispose()
        {
            dotNetInstance?.Dispose();
            AppState.PropertyChanged -= HandleCodePropertyChanged;
            Console.WriteLine("RazorCodeHome.razor disposed");
        }
    }
}
