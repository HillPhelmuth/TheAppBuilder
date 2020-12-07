using System.Collections.Generic;
using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components.RazorProject
{
    public partial class RazorSamplesModal : ComponentBase
    {
        [Parameter]
        public ProjectFile ActiveProjectFile { get; set; }
        [Inject]
        protected IModalDialogService ModalService { get; set; }

        protected void UpdateActiveFile(KeyValuePair<string, string> selectedFile)
        {
            ActiveProjectFile = new ProjectFile { Name = $"{selectedFile.Key}.razor", Content = selectedFile.Value, FileType = FileType.Razor };
            var parameters = new ModalDialogParameters { { "ActiveCodeFile", ActiveProjectFile } };
            ModalService.Close(true, parameters);
        }
    }
}
