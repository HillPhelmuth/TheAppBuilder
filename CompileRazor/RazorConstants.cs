namespace AppBuilder.CompileRazor
{
    public static class RazorConstants
    {
        public const string DefaultComponentName = "__RazorOutput.razor";
        public const string DefaultFileContent = @"<h1>Hello World</h1>

@code {

}
";
        public const string MainComponentCodePrefix = "@page \"/__razorOutput\"\n";
        public const string MainComponentPagePath = "/__razorOutput";
    }
}
