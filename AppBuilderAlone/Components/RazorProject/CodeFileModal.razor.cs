using System.Threading.Tasks;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components.RazorProject
{
    public partial class CodeFileModal : ComponentBase
    {
        [Inject]
        protected IModalDialogService ModalService { get; set; }
        [Inject]
        protected AppState AppState { get; set; }

        protected void UpdateActiveFile(ProjectFile selectedFile)
        {
            var parameters = new ModalDialogParameters {{"FileAction", "select"}, {"ActiveCodeFile", selectedFile}};
            ModalService.Close(true, parameters);
        }

        private async Task DeleteFile(ProjectFile selectedFile)
        {
            var response = await ModalService.ShowMessageBoxAsync("Delete", $"Are you sure you want to delete {selectedFile.Name} from project?",
                MessageBoxButtons.YesNo);
            if (response != MessageBoxDialogResult.Yes) return;
            var parameters = new ModalDialogParameters {{"FileAction", "delete"}, {"DeletedCodeFile", selectedFile}};
            ModalService.Close(true, parameters);
        }
    }
}
