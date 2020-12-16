using AppBuilder.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBuilder.CompileConsole
{
    public static class ConsoleSampleProject
    {
        public static List<ProjectFile> IntroProject => new List<ProjectFile>
        {
            new ProjectFile
            {
                Name = ConsoleConstants.DefaultConsoleName,
                Content = ConsoleConstants.SampleProjectMain,
                FileType = FileType.Class
            },
            new ProjectFile
            {
                Name = "Test.cs",
                FileType = FileType.Class,
                Content = ConsoleConstants.SampleProjectTest
            }
        };
    }
}
