namespace AppBuilder.Shared
{
    public static class CodeSnippets
    {
        public const string ConsoleInput = "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = Console.ReadLine();\n\t\tstring additional = \" was your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";

        public const string ConsoleMutlitpeWrites =
            "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = \"Foo\";\n\t\tstring additional = \" was not your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";
        public const string RazorSnippet = @"<h1>Hello World</h1>
<h3>@testString</h3>
@code
{
    string testString = ""I Am Here!"";
}";
        public const string RazorChild = @"<h1>Hello World</h1>
<h3>@testString</h3>
@code
{
    string testString = ""I Am Child!"";
}";
        public const string RazorParent = @"<h1>Hello World</h1>
<h3>@testString</h3>
<RazorChild></RazorChild>
@code
{
    string testString = ""I Am Parent!"";
}";

        public const string RazorActive = @"<h1 class=""@cssClass1"">Hello World</h1>
        <h3 class=""@cssClass2"">@testString</h3>
        <style>
        .redBlue {
            color:#d50000;
            background-color:#4fc3f7;
        }
        .blueRed {
            color:#4fc3f7;
            background-color:#d50000;
        }
        .big
        {

        }
        .small
        {

        }
        </style >
        <button @onclick = ""GetRandom"" > Click </button >
        <button @onclick = ""ChangeColor"" > @labelText </button >
        @if(randomVal > 50)
        {
            <p > Value @randomVal is greater than 50 </p >
        }
        @if(randomVal <= 50)
        {
            <p > Value @randomVal is less than 50 </p >
        }
        @code
        {
            string blueRed = ""blueRed"";
            string redBlue = ""redBlue"";
            string cssClass1 = "" "";
            string cssClass2 = """";
            string testString = ""I Am here!"";
            Random random = new Random();
            int randomVal = 0;
            string labelText = ""START COLOR"";

            public void GetRandom()
            {
                randomVal = random.Next(1, 111);
                StateHasChanged();
            }
            public void ChangeColor()
            {
                labelText = ""Change Color"";
                cssClass1 = cssClass1 == redBlue ? blueRed : redBlue;
                cssClass2 = cssClass2 == blueRed ? redBlue : blueRed;

            }

        }";
    }
}
