namespace CompileRazor
{
    public static class DefaultStrings
    {
        public const string MainComponentFilePath = "__RazorOutput.razor";
        public const string MainComponentDefaultFileContent = @"<h1>Hello World</h1>

@code {

}
";
        public const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        public const string MainComponentPagePath = "/__razorOutput";
        
    }
}
