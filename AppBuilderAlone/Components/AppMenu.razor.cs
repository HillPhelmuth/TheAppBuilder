using AppBuilder.Client.Components.RazorProject;
using AppBuilder.Client.Services;
using AppBuilder.CompileConsole;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppBuilder.Client.Components
{
    public partial class AppMenu : ComponentBase
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private ZipService ZipService { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        public IJSRuntime JsRuntime { get; set; }
        [Parameter]
        public ProjectType ProjectType { get; set; }
        private RazorInterop RazorInterop => new RazorInterop(JsRuntime);

        private async Task DownloadProjectAsZip()
        {
            var currentProject = AppState.ActiveProject;
            var projectZipBytes = await ZipService.ZipUpFiles(currentProject.Files);
            await RazorInterop.DownloadProjectFile($"{currentProject.Name}.zip", projectZipBytes);
            ModalService.Close(true);
        }
        private async Task CreateFile()
        {
            var inputForm = new ModalDataInputForm("Create new conponent", "what should we call this component?");
            var nameField = inputForm.AddStringField("Name", "Component Name", "");
            var languageField = inputForm.AddEnumField("FileType", "File type", FileType.Razor);
            string filename = string.Empty;
            string fileType = "razor";
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (!await inputForm.ShowAsync(ModalService, options)) return;
            if (string.IsNullOrEmpty(nameField.Value)) return;
            fileType = languageField.Value.AsString();
            filename = filename.Contains(".razor") || filename.Contains(".cs")
                ? nameField.Value
                : $"{nameField.Value}.{fileType}";

            var sampleSnippet = languageField.Value == FileType.Razor ? $"<h1>{filename}</h1>" : $"class {filename}\r{{\r\t\r}}";
            AppState.CreateProjectFile(filename, sampleSnippet, languageField.Value);
            ModalService.Close(true);
        }
        private async Task LoadSampleProject()
        {
            var inputForm = new ModalDataInputForm("Load Sample Project", "What type of project?");
            var typeField = inputForm.AddEnumField("FileType", "Project type", ProjectType.Blazor);
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (!await inputForm.ShowAsync(ModalService, options)) return;

            var sampleProjectFiles = typeField.Value == ProjectType.Blazor ? RazorSampleProjects.IntroProject : ConsoleSampleProject.IntroProject;
            ProjectFile sampleMain = new ProjectFile();
            switch (typeField.Value)
            {
                case ProjectType.Blazor:
                    sampleMain = sampleProjectFiles.FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName);
                    break;
                case ProjectType.Console:
                    sampleMain = sampleProjectFiles.FirstOrDefault(x => x.Name == ConsoleConstants.DefaultConsoleName);
                    break;
            }
            AppState.ActiveProject = new UserProject { Name = "SAMPLE", Files = sampleProjectFiles };
            AppState.ProjectFiles = sampleProjectFiles;
            AppState.ActiveProjectFile = sampleMain;
            //AppState.CodeSnippet = sampleMain?.Content;
            ModalService.Close(true);
        }
        private void HandleProjectUpload(List<ProjectFile> projectFiles)
        {
            AppState.ProjectFiles = projectFiles;
            AppState.ActiveProjectFile = projectFiles.FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName || x.Name == ConsoleConstants.DefaultConsoleName);
            AppState.ActiveProject.Files.AddRange(projectFiles);
            ModalService.Close(true);
        }
        protected async Task SelectActiveFile()
        {
            if (AppState.HasActiveProject)
            {
                AppState.ProjectFiles = AppState.ActiveProject.Files;
            }

            await Task.Delay(10);
            var result = await ModalService.ShowDialogAsync<CodeFileModal>("Select a code snippet");
            if (result.Success)
            {
                var codeFile = result.ReturnParameters.Get<ProjectFile>("ActiveCodeFile");
                //isCSharp = codeFile.FileType == FileType.Class;
                await InvokeAsync(StateHasChanged);
                await Task.Delay(50);
                AppState.ActiveProjectFile = codeFile;
                //AppState.CodeSnippet = codeFile.Content;
            }
            ModalService.Close(true);
        }
        private async Task SaveProject()
        {
            //if (!AppStateService.HasActiveProject) return;
            //var projectFiles = AppStateService.ActiveProject;
            //var apiResponse = await PublicClient.SaveCurrentFiles(projectFiles, AppStateService.UserName);
        }
        private async Task CreateProject()
        {
            var inputForm = new ModalDataInputForm("Create new Project", "Please give your project a name");
            var nameField = inputForm.AddStringField("Name", "Project Name", "");
            var typeField = inputForm.AddEnumField("ProjectType", "Project Type", ProjectType.Blazor);
            var useCurrentFiles = inputForm.AddBoolField("UseCurrentFiles", "Add current files to project", true);
            string projectName = "";
            bool useFiles = true;
            var options = new ModalDialogOptions()
            {
                Style = "small-modal"
            };
            if (!await inputForm.ShowAsync(ModalService, options)) return;
            projectName = nameField.Value;
            useFiles = useCurrentFiles.Value;
            var defaultFile = typeField.Value == ProjectType.Blazor ?
                new ProjectFile { Name = RazorConstants.DefaultComponentName, Content = RazorConstants.DefaultFileContent, FileType = FileType.Razor } :
                new ProjectFile { Name = ConsoleConstants.DefaultConsoleName, Content = ConsoleConstants.DefaultSnippet, FileType = FileType.Class };
            var newProject = new UserProject
            {
                Name = projectName,
                Files = useFiles ? AppState.ProjectFiles : new List<ProjectFile> { defaultFile }
            };

            AppState.ActiveProject = newProject;
            AppState.ProjectFiles = newProject.Files;
            ModalService.Close(true);
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
            AppState.ActiveProjectFile.Content = code;
            //AppState.CodeSnippet = code;
            ModalService.Close(true);
        }

    }
}
