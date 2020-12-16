using Blazor.ModalDialog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBuilder.CompileConsole
{
    public class ConsoleCustomRead : StreamReader
    {
        private IModalDialogService _modalService;
        public ConsoleCustomRead(string input, IModalDialogService modalService)
            : base(input)
        {
            _modalService = modalService;
        }
        public override async Task<string> ReadLineAsync()
        {
            ModalDataInputForm frm = new ModalDataInputForm("Console Readline", "Input");
            var readlineField = frm.AddStringField("readline", "Console Readline", "");
            if (!await frm.ShowAsync(_modalService)) return string.Empty;
            return readlineField.Value;
        }
        public override string ReadLine()
        {
            return ReadLineAsync().Result;
        }
    }
}
