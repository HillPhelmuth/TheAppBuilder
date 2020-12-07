using System.Collections.Generic;
using AppBuilder.Shared;

namespace AppBuilder.CompileRazor
{
    public static class RazorSampleProjects
    {
        public static List<ProjectFile> IntroProject => new List<ProjectFile>
        {
            new ProjectFile
            {
                Name = RazorConstants.DefaultComponentName,
                FileType = FileType.Razor,
                Content = @"<h1>Hello World</h1>
<h3>@testString</h3>
<button @onclick=""GetTestString"">Test</button>
<Child ChangeParent=""HandleChildString""/>
@code
{
    string testString = ""I Am Here!"";
    void GetTestString()
    {
        testString = testString == Test.GetString() ? ""I Am Here"" : Test.GetString();
    }
    void HandleChildString(string str)
    {
        testString = str;
    }
}"
            },
            new ProjectFile
            {
                Name = "Child.razor",
                FileType = FileType.Razor,
                Content = @"<button @onclick=""SayHello"">Child Button</button>

@code {
    [Parameter]
    public EventCallback<string> ChangeParent {get;set;}
    void SayHello()
    {
        ChangeParent.InvokeAsync(""Hello From Child Component"");
    }

}"
            },
            new ProjectFile
            {
                Name = "Test.cs",
                FileType = FileType.Class,
                Content = @"public class Test
{
	public static string GetString()
    {
        return ""Hello From Test Class"";
    }
}"
            }
        };

    }
}
