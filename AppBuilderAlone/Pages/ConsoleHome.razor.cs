using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using MatBlazor;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppBuilder.Client.Pages
{
    public partial class ConsoleHome : ComponentBase, IDisposable
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private ConsoleCompile CompileService { get; set; }
        [Inject]
        private IModalDialogService ModalService { get; set; }
              

        private ProjectFile activeFile;
        private string fileName;
        private bool isCodeCompiling;
        private string ReadlinePattern { get; } = "Console.ReadLine()";
        
        protected override async Task OnInitializedAsync()
        {
            await CompileService.InitAsync();
            activeFile = new ProjectFile { Name = ConsoleConstants.DefaultConsoleName, Content = ConsoleConstants.DefaultSnippet, FileType = FileType.Class };
            AppState.ActiveProject ??= new UserProject { Name = "DefaultConsole", Files = new List<ProjectFile> { activeFile } };
            AppState.ActiveProjectFile = activeFile;
            AppState.Language = "csharp";
            AppState.PropertyChanged += HandleCodePropertyChanged;
            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                AppState.ActiveProjectFile ??= activeFile;
                //AppState.CodeSnippet = ConsoleConstants.DefaultSnippet;

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
            AppState.ActiveProjectFile.Content = code;
            AppState.SaveCode(AppState.ActiveProjectFile);
            activeFile.Content = code;
            await Task.Delay(50);
            AppState.CurrentOutput = await CompileService.CompileAndRun(AppState.ProjectFiles.ToArray());
            isCodeCompiling = false;
            await InvokeAsync(StateHasChanged);
        }
        private void HandleSave(string content)
        {
            AppState.ActiveProjectFile.Content = content;
            var currentFile = AppState.ActiveProjectFile;
            currentFile.Content = content;
            AppState.SaveCode(currentFile);
            if (!AppState.HasActiveProject) return;

            AppState.ActiveProject.Files = AppState.ProjectFiles;
        }
        private void UpdateName()
        {
            AppState.ChangeFileName(fileName);
        }
        private void HandleTabFileChange(BaseMatTabLabel baseMatTab)
        {
            AppState.ActiveProjectFile = AppState.ProjectFiles.FirstOrDefault(x => x.Name == baseMatTab.Id);
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
    public static class StringExtension
    {
        public static List<int> AllIndexesOf(this string str, string value)
        {
            if (string.IsNullOrEmpty(value))
                return new List<int>();
            var indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.Ordinal);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }
    }
}
