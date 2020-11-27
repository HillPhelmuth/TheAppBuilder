using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;
using Shared;

namespace AppBuilderAlone.Pages
{
    public partial class RazorCodeHome
    {
        [Inject]
        private AppState AppState { get; set; }

        #region Monaco Editor

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
                Value = AppState.CodeSnippet ?? "private string MyProgram() \n" +
                        "{\n" +
                        "    string input = \"this does not\"; \n" +
                        "    string modify = input + \" suck!\"; \n" +
                        "    return modify;\n" +
                        "}\n" +
                        "return MyProgram();"
            };
        }
        protected MonacoEditor Editor { get; set; }
        protected async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await Editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });
            await Editor.AddAction("saveAction", "Save Snippet", new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_S }, null, null, "navigation", 1.5, async (editor, keyCodes) =>
            {
                //await AddSnippetToUser();
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
            await Editor.AddAction("executeAction", "Execute Code",
                new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.Enter }, null, null, "navigation", 2.5,
                async (editor, keyCodes) =>
                {
                    //await SubmitCode();
                    Console.WriteLine("Code Executed from Editor Command");
                });
            await Editor.SetValue(AppState.CodeSnippet);
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

            decorationIds = await Editor.DeltaDecorations(null, newDecorations);
        }
        private string[] decorationIds;

        protected void OnContextMenu(EditorMouseEvent eventArg)
        {

            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }
        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value.ToString()}");
            await MonacoEditorBase.SetTheme(e.Value.ToString());
        }

       
        #endregion

    }
}
