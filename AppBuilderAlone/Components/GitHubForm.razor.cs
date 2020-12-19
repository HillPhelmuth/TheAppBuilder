using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components
{
    public partial class GitHubForm
    {
        [Inject]
        public AppState AppStateService { get; set; }
        [Inject]
        public IModalDialogService ModalService { get; set; }
        [Inject]
        public GithubClient GithubClient { get; set; }
        public GitHubFormModel FormModel { get; set; } = new();

        private async Task Submit()
        {
            var codeFile = await GithubClient.CodeFromPublicRepo(FormModel.GithubName, FormModel.RepoName,
                FormModel.FilePath);
            var parameters = new ModalDialogParameters
            {
                {"FileCode", codeFile}
            };
            ModalService.Close(true, parameters);
        }
    }

    public class GitHubFormModel
    {
        [Required]
        public string GithubName { get; set; }
        [Required]
        public string RepoName { get; set; }
        [Required]
        public string FilePath { get; set; }
    }
}
