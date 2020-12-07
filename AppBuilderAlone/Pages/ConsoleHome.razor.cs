using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Transactions;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppBuilder.Client.Pages
{
    public partial class ConsoleHome : ComponentBase, IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private CSharpCompile CompileService { get; set; }

        private ProjectFile activeFile;
        private string fileName;
        private bool isCodeCompiling;

        protected override async Task OnInitializedAsync()
        {
            await CompileService.Init();
            AppState.Language = "csharp";
            AppState.PropertyChanged += HandleCodePropertyChanged;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                AppState.CodeSnippet = ConsoleConstants.DefaultSnippet;

                //await JsRuntime.RazorAppInit(dotNetInstance);
            }
            await base.OnAfterRenderAsync(firstRender);
        }
        private void HandleCodeSubmit(string code)
        {
            isCodeCompiling = true;
            StateHasChanged();
            _ = CodeSubmit(code);
        }
        private async Task CodeSubmit(string code)
        {
            activeFile ??= new ProjectFile { Name = fileName, FileType = FileType.Class };
            activeFile.Content = code;
            await Task.Delay(100);
            AppState.CurrentOutput = await CompileService.CompileAndRun(activeFile);
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);
        }
        private void UpdateName()
        {
            AppState.ChangeFileName(fileName);
        }
        private void HandleCodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.ActiveProjectFile) && args.PropertyName != nameof(AppState.ActiveProject)) return;
            AppState.CodeSnippet = AppState?.ActiveProjectFile?.Content;
            AppState.ProjectFiles = AppState?.ActiveProject?.Files;
            StateHasChanged();
        }

        public void Dispose()
        {
            AppState.PropertyChanged -= HandleCodePropertyChanged;
        }
    }
}
