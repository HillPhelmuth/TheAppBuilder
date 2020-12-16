namespace AppBuilder.CompileConsole
{
    public static class ConsoleConstants
    {
        public const string DefaultSnippet = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
  public static void Main()
  {
    foreach (var i in Fibonacci().Take(20))
    {
      Console.WriteLine(i);
    }
  }

  private static IEnumerable<int> Fibonacci()
  {
    int current = 1, next = 1;

    while (true) 
    {
      yield return current;
      next = current + (current = next);
    }
  }
}
";
        public const string SampleProjectMain = @"using System;
using System.Collections.Generic;
using System.Linq;

public class Program
{
  public static void Main()
  {
    foreach (var i in Test.Fibonacci().Take(20))
    {
      Console.WriteLine(i);
    }
  }
}";
        public const string SampleProjectTest = @"using System.Collections.Generic;
using System.Linq;
using System;
public static class Test
{
  public static IEnumerable<int> Fibonacci()
  {
    int current = 1, next = 1;

    while (true) 
    {
      yield return current;
      next = current + (current = next);
    }
  }
}";
        public const string ConsoleInput = "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = Console.ReadLine();\n\t\tstring additional = \" was your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";

        public const string ConsoleMutlitpeWrites =
            "using System;\n\nclass Program\n{\n\tpublic static void Main()\n\t{\n\t\tstring input = \"Foo\";\n\t\tstring additional = \" was not your input\";\n\t\tConsole.WriteLine(input);\n\t\tstring newOutput = input + additional;\n\t\tConsole.WriteLine(newOutput);\n\t}\n}";
        public const string DefaultConsoleName = "Program.cs";
    }
}
