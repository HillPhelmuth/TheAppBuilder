﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AppBuilder.CompileConsole;
using AppBuilder.Shared;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;

namespace AppBuilder.Client.Components
{
    public partial class Editor
    {
        [Inject]
        private AppState AppState { get; set; }
        [Parameter]
        public string Language { get; set; }
        [Parameter]
        public string CodeSnippet { get; set; }
        [Parameter]
        public EventCallback<string> OnCodeSubmit { get; set; }
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
            return base.OnAfterRenderAsync(firstRender);
        }

        public async Task SubmitCode()
        {
            var currentCode = await MonacoEditor.GetValue();
            await OnCodeSubmit.InvokeAsync(currentCode);
        }

        protected async void UpdateSnippet(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(AppState.CodeSnippet)) return;
            await MonacoEditor.SetValue(AppState.CodeSnippet);
        }
        #region Monaco Editor
        protected MonacoEditor MonacoEditor { get; set; }
        protected StandaloneEditorConstructionOptions EditorOptionsRoslyn(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                AutoIndent = true,
                //HighlightActiveIndentGuide = true,
                ColorDecorators = true,
                Minimap = new MinimapOptions { Enabled = false },
                Hover = new HoverOptions { Delay = 400 },
                Find = new FindOptions { AutoFindInSelection = true, SeedSearchStringFromSelection = true, AddExtraSpaceOnTop = true },
                Lightbulb = new LightbulbOptions { Enabled = true },
                AcceptSuggestionOnEnter = "smart",
                Language = Language,
                Value = AppState.CodeSnippet ?? ConsoleConstants.DefaultSnippet
            };
        }

        protected async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await MonacoEditor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
            await MonacoEditor.AddAction("saveAction", "Save Snippet", new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null, null, "navigation", 1.5, async (editor, keyCodes) =>
            {
                //await AddSnippetToUser();
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
            await MonacoEditor.AddAction("executeAction", "Execute Code",
                new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter }, null, null, "navigation", 2.5,
                async (editor, keyCodes) =>
                {
                    //await SubmitCode();
                    Console.WriteLine("Code Executed from Editor Command");
                });

            await MonacoEditor.SetValue(AppState.CodeSnippet);
            var newDecorations = new[]
            {
                new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Bridge.Range(3,1,3,1),
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = false,
                        ClassName = "decorationContentClass",
                        GlyphMarginClassName = "decorationGlyphMarginClass"
                    }
                }
            };

            decorationIds = await MonacoEditor.DeltaDecorations(null, newDecorations);
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
        #endregion
    }
}
