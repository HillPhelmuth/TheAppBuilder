using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Client.Components;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

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
        private List<ProjectFile> ExtractedFiles { get; set; } = new List<ProjectFile>();

        private string orgName;
        private string repoName;
        private string fileName;

        protected override async Task OnInitializedAsync()
        {
            AppState.PropertyChanged += HandleCodePropertyChanged;
            await base.OnInitializedAsync();
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
        private void HandleCodePropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != "ProjectFiles" && args.PropertyName != "ActiveProject") return;
            ExtractedFiles = AppState.ProjectFiles;

            StateHasChanged();
        }
        
        public void Dispose()
        {
            AppState.PropertyChanged -= HandleCodePropertyChanged;
        }
    }
}
