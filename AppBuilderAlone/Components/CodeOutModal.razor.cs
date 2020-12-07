using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components
{
    public partial class CodeOutModal
    {
        [Inject]
        public IModalDialogService ModalService { get; set; }
        [Inject]
        public AppState AppState { get; set; }
        [Parameter]
        public string CodeOutput { get; set; }

        private void ClearOutput()
        {
            AppState.CurrentOutput = string.Empty;
        }
    }
}
