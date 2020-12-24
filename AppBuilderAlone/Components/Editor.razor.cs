using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AppBuilder.Client.Services;
using AppBuilder.CompileConsole;
using AppBuilder.Shared;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppBuilder.Client.Components
{
    public partial class Editor
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IJSRuntime JsRuntime { get; set; }
        private RazorInterop RazorInterop => new(JsRuntime);
        [Parameter]
        public string Language { get; set; }
        [Parameter]
        public string CodeSnippet { get; set; }
        [Parameter]
        public EventCallback<string> OnCodeSubmit { get; set; }
        [Parameter]
        public EventCallback<string> OnSave { get; set; }
        [Parameter]
        public string ButtonLabel { get; set; }
        protected override Task OnInitializedAsync()
        {
            Language ??= "csharp";
            ButtonLabel ??= "Run";
            MonacoEditor = new MonacoEditor();


            return base.OnInitializedAsync();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                AppState.PropertyChanged += UpdateSnippet;
            Console.WriteLine("Editor.razor renders");
            return base.OnAfterRenderAsync(firstRender);
        }

        public async Task SubmitCode()
        {
            var currentCode = await MonacoEditor.GetValue();
            await OnCodeSubmit.InvokeAsync(currentCode);
        }
        public async Task SaveCode()
        {
            var currentCode = await MonacoEditor.GetValue();
            await OnSave.InvokeAsync(currentCode);
        }
        protected async void UpdateSnippet(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.CodeSnippet) && args.PropertyName != nameof(AppState.ActiveProjectFile)) return;
            var newCode = args.PropertyName == nameof(AppState.ActiveProjectFile) ? AppState.ActiveProjectFile.Content : AppState.CodeSnippet;
            await MonacoEditor.SetValue(newCode);
            StateHasChanged();
        }
        #region Monaco Editor
        protected MonacoEditor MonacoEditor { get; set; }
        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(MonacoEditor editor)
        {
            return new()
            {
                AutomaticLayout = true,
                AutoIndent = true,
                ColorDecorators = true,
                Minimap = new MinimapOptions { Enabled = false },
                Hover = new HoverOptions { Delay = 400 },
                Find = new FindOptions { AutoFindInSelection = true, SeedSearchStringFromSelection = true, AddExtraSpaceOnTop = true },
                Lightbulb = new LightbulbOptions { Enabled = true },
                AcceptSuggestionOnEnter = "smart",
                Language = Language ?? "csharp",
                Value = AppState.ActiveProjectFile?.Content ?? ConsoleConstants.DefaultSnippet
            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await MonacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (mEdit, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
            await MonacoEditor.AddAction("saveAction", "Save Snippet", new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null, null, "navigation", 1.5, async (mEdit,keyCodes) =>
            {
                
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
            await MonacoEditor.AddAction("executeAction", "Execute (ctrl + enter)",
                new[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter }, null, null, "navigation", 2.5,
                async (mEdit, keyCodes) =>
                {
                    await SubmitCode();
                    Console.WriteLine("Code Executed from Editor Command");
                });
            await MonacoEditor.AddAction("copyAction", "Copy to clipboard (ctrl + c)",
                new[] {(int) KeyMode.CtrlCmd | (int) KeyCode.KEY_D}, null, null, "navigation", 3.5,
                async (mEdit, keyCodes) => await CopyToClipboard());
            await MonacoEditor.SetValue(AppState.CodeSnippet);
            //var newDecorations = new[]
            //{
            //    new ModelDeltaDecoration
            //    {
            //        Range = new BlazorMonaco.Bridge.Range(3,1,3,1),
            //        Options = new ModelDecorationOptions
            //        {
            //            IsWholeLine = false,
            //            ClassName = "decorationContentClass",
            //            GlyphMarginClassName = "decorationGlyphMarginClass"
            //        }
            //    }
            //};

            //decorationIds = await MonacoEditor.DeltaDecorations(null, newDecorations);
        }
        private string[] decorationIds;

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }
        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value}");
            await MonacoEditorBase.SetTheme(e.Value?.ToString());
        }

        private async Task CopyToClipboard()
        {
            var text = await MonacoEditor.GetValue();
            Console.WriteLine($"Text copied to ClipBoard:\r\n{text}");
            await RazorInterop.CopyToClipboard(text);
        }
        #endregion
    }
}
