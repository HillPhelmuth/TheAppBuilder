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
using Blazored.LocalStorage;
using AppBuilder.Client.StaticCustomAuth.Interfaces;

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
        [Inject]
        private StorageClient StorageClient { get; set; }
        [Inject]
        private ISyncLocalStorageService LocalStorage { get; set; }

        private RazorInterop RazorInterop => new(JsRuntime);

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
            ModalService.Close(true);
        }
        private void HandleProjectUpload(UserProject project)
        {
            if (project == null || project.Files == null) return;
            AppState.ActiveProject = project;
            AppState.ProjectFiles = project.Files;
            AppState.ActiveProjectFile = project.Files.FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName || x.Name == ConsoleConstants.DefaultConsoleName);

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
                var action = result.ReturnParameters.Get("FileAction", "select");
                if (action == "select")
                {
                    var codeFile = result.ReturnParameters.Get("ActiveCodeFile", AppState.ActiveProjectFile);
                    AppState.ActiveProjectFile = codeFile;
                }

                if (action == "delete")
                {
                    var deletedFile = result.ReturnParameters.Get<ProjectFile>("DeletedCodeFile");
                    AppState.ActiveProject.Files.Remove(deletedFile);
                    AppState.ProjectFiles.Remove(deletedFile);
                    if (AppState.ActiveProjectFile == deletedFile)
                        AppState.ActiveProjectFile = AppState.ProjectFiles[0];
                }
                await InvokeAsync(StateHasChanged);
               
            }
            ModalService.Close(true);
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

        private async Task GetProjectsFromStorage()
        {
            if (!await AlertOffline()) return;
            var username = AppState.CurrentUser;
            var userProjects = await StorageClient.GetUserProjects(username);
            var parameters = new ModalDialogParameters { { "ProjectNames", userProjects } };
            var option = new ModalDialogOptions { Style = "modal-dialog-appMenu" };
            var result = await ModalService.ShowDialogAsync<UserProjects>("User Projects", option, parameters);
            if (!result.Success) return;
            var selectedProject = result.ReturnParameters.Get<string>("SelectedProject", "none");
            if (selectedProject == "none") return;
            var userProject = await StorageClient.GetProject(username, selectedProject);
            AppState.ActiveProject = userProject;
            AppState.ProjectFiles = userProject.Files;
            ModalService.Close(true);
        }

        private async Task SaveToCloud()
        {
            if (!AppState.HasActiveProject || !AppState.IsAuthUser) return;
            if (!await AlertOffline()) return;
            var username = AppState.CurrentUser;
            var activeProject = AppState.ActiveProject;
            var responseString = await StorageClient.UploadProject(username, activeProject);
            Console.WriteLine($"Response from Upload Function:\r\n{responseString}");
            ModalService.Close(true);
        }
        protected async Task UpdateFromPublicRepo()
        {
            if (!await AlertOffline()) return;
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

        private async Task<bool> AlertOffline()
        {
            if (AppState.IsOnline) return true;
            LocalStorage.SetItem($"{AppState.CurrentUser}_{nameof(AppState)}", AppState);
            var result = await ModalService.ShowMessageBoxAsync("Currently Offline",
                 @"It looks like your network status is curently offline. Don't worry, your current project is saved to local storage. Try again when you're connected to the internet.

Would you like to download your project files?", MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button1);

            if (result == MessageBoxDialogResult.Yes)
            {
                await DownloadProjectAsZip();
            }

            return false;
        }
    }
}
