using System.Linq;
using System.Threading.Tasks;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components.RazorProject
{
    public partial class ProjectCrudModal
    {
        [Inject]
        public AppState AppState { get; set; }
        //[Inject]
        //public AppState AppState { get; set; }
        [Inject]
        public IModalDialogService ModalDialogService { get; set; }

        protected void SelectUserProject(UserProject project)
        {
            AppState.ActiveProject = project;
            AppState.ProjectFiles = project.Files;
            AppState.ActiveProjectFile =
                project.Files.FirstOrDefault(x => x.Name == RazorConstants.MainComponentFilePath);
            AppState.CodeSnippet = AppState.ActiveProjectFile?.Content ?? "EMPTY";
            ModalDialogService.Close(true);
        }

        protected async Task DeleteUserProject(UserProject project)
        {

        }
    }
}
