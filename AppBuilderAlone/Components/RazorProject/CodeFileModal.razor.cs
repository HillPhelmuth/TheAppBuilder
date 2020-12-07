using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components.RazorProject
{
    public partial class CodeFileModal : ComponentBase
    {
        [Parameter]
        public ProjectFile ActiveProjectFile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected AppState AppState { get; set; }

        protected void UpdateActiveFile(ProjectFile selectedFile)
        {
            var parameters = new ModalDialogParameters();
            ActiveProjectFile = selectedFile;
            parameters.Add("ActiveCodeFile", selectedFile);
            ModalService.Close(true, parameters);
        }
    }
}
