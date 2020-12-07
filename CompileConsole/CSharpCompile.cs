using AppBuilder.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppBuilder.CompileConsole
{
    public class CSharpCompile
    {
        private readonly List<MetadataReference> _references = new();
        private readonly IDependencyResolver _dependencyResolver;
        private static CSharpCompilation _baseCompilation;

        public CSharpCompile(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public List<string> Logs { get; } = new();

        public async Task Init()
        {
            if (_references.Any())
            {
                return;
            }

            _references.AddRange(await _dependencyResolver.GetAssemblies());
            //Array.Empty<SyntaxTree>()
            _baseCompilation = CSharpCompilation.Create(
                "AppBuilderAlone.Demo",
                Array.Empty<SyntaxTree>(),
                _references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        }

        public async Task<string> CompileAndRun(params ProjectFile[] csharpFiles)
        {
            Logs.Clear();
            var filesString = JsonSerializer.Serialize(csharpFiles);
            Console.WriteLine($"Files json: \r\n {filesString}");
            var assembly = await Compile(csharpFiles);

            return Run(assembly);
        }

        public async Task<Assembly> Compile(params ProjectFile[] csharpFiles)
        {
            // Make sure the needed assembly references are available.
            await Init();

            // Convert the C# files into syntax trees.
            IEnumerable<SyntaxTree> syntaxTrees = new List<SyntaxTree>();

            //try
            //{
            var source = csharpFiles[0].Content;
            var compilation2 = CSharpCompilation.Create("DynamicCode")
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                .AddReferences(_references)
                .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)));

            syntaxTrees = csharpFiles
            .Select(x => CSharpSyntaxTree
            .ParseText(x.Content, new CSharpParseOptions(LanguageVersion.Preview)/*, x.Name*/));
            //}
            //catch (Exception ex)
            //{
            //    Logs.Add($"Compile failed.\r\n{ex.Message}\r\n\t{ex.StackTrace}");
            //}

            _baseCompilation.AddSyntaxTrees(syntaxTrees);
            // Create a new compilation with the source code and the assembly references.
            var compilation = compilation2; /*_baseCompilation;*/

            await using var stream = new MemoryStream();

            // Emit the IL for the compiled source code into the stream.
            var result = compilation.Emit(stream);

            foreach (var diagnostic in result.Diagnostics)
            {
                Logs.Add(diagnostic.ToString());
            }

            if (!result.Success)
            {
                Logs.Add("");
                Logs.Add("Build FAILED.");
                Logs.Add($"Error: {string.Join(", ", result.Diagnostics.Select(x => x.GetMessage()))}");
                Console.WriteLine(string.Join("\r\n", Logs));
                throw new CSharpCompilationException();
            }

            Logs.Add("");
            Logs.Add("Build succeeded.");

            // Reset stream to beginning.
            stream.Seek(0, SeekOrigin.Begin);

            // Load the newly created assembly into the current application domain.
            var assembly = AppDomain.CurrentDomain.Load(stream.ToArray());

            return assembly;
        }

        public static string Run(Assembly assembly)
        {
            // Capture the Console outputs.
            using var sw = new StringWriter();
            Console.SetOut(sw);

            var main = assembly.EntryPoint;

            var parameters = main.GetParameters().Any()
                ? new object[] { Array.Empty<string>() }
                : null;

            main.Invoke(null, parameters);

            return sw.ToString();
        }
    }
}
