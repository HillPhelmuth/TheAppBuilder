using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;

namespace AppBuilder.CompileRazor
{
    public class CodeAssemblyModel
    {
        public Compilation Compilation { get; set; }

        public byte[] AssemblyBytes { get; set; }

        public string AssemblyString => Convert.ToBase64String(AssemblyBytes ?? new ReadOnlySpan<byte>()) ?? "Empty";

        public IEnumerable<CustomDiag> Diagnostics { get; set; }
    }

    public class RazorToCSharpModel
    {
        public RazorProjectItem ProjectItem { get; set; }

        public string Code { get; set; }
        public IEnumerable<CustomDiag> Diagnostics { get; set; }
    }
}
