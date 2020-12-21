using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using AppBuilder.Shared;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.JSInterop;

namespace AppBuilder.CompileRazor
{
    public class RazorCompile
    {
        private readonly AppState _appState;
       
        private readonly HttpClient httpClient;
        public RazorCompile(AppState appState, HttpClient httpClient)
        {
            _appState = appState;
          
            this.httpClient = httpClient;
        }
        private static CSharpCompilation _baseCompilation;
        private IEnumerable<MetadataReference> _references;

        public const string DefaultRootNamespace = "AppBuilder.Output";

        private const string WorkingDirectory = "/BlazorOut/";
        private readonly RazorProjectFileSystem _fileSystem = new WebRazorProjectFileSystem();
        private readonly RazorConfiguration _config = RazorConfiguration.Create(
            RazorLanguageVersion.Latest,
            "Blazor",
            extensions: Array.Empty<RazorExtension>());

        public async Task<CodeAssemblyModel> CompileToAssemblyAsync(ICollection<ProjectFile> codeFiles)
        {
            Console.WriteLine($"CompileToAssemblyAsync: {string.Join("\r\n", codeFiles.Select(x => x.Name))}");
            if (codeFiles == null)
            {
                throw new ArgumentNullException(nameof(codeFiles));
            }

            var cSharpResults = await ConvertRazorToCSharp(codeFiles);
            var result = GetCodeAssembly(new List<RazorToCSharpModel>(cSharpResults));
            return result;
        }

        private async Task<ICollection<RazorToCSharpModel>> ConvertRazorToCSharp(ICollection<ProjectFile> codeFiles)
        {
            // The first phase won't include any metadata references for component discovery. This mirrors what the build does.
            Console.WriteLine($"ConvertRazorToCSharp: {string.Join("\r\n", codeFiles.Select(x => x.Name))}");
            var projectEngine = CreateRazorProjectEngine(Array.Empty<MetadataReference>());
            var declarations = new List<RazorToCSharpModel>();
            foreach (var codeFile in codeFiles.Where(x => x.Name.EndsWith(".razor")))
            {
                RazorProjectItem projectItem = CreateRazorProjectItem(codeFile.Name, codeFile.Content);

                RazorCodeDocument codeDocument = projectEngine.ProcessDeclarationOnly(projectItem);
                RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();

                declarations.Add(new RazorToCSharpModel
                {
                    ProjectItem = projectItem,
                    Code = cSharpDocument.GeneratedCode,
                    Diagnostics = cSharpDocument.Diagnostics.Select(diagnostic => new CustomDiag(diagnostic)),
                });
            }
            //ToDo Likely Solution
            foreach (var codeFile in codeFiles.Where(x => x.Name.EndsWith(".cs")))
            {
                declarations.Add(new RazorToCSharpModel
                {
                    Code = codeFile.Content,
                    Diagnostics = new List<CustomDiag>()
                });
            }
            // Get initial core assembly
            var tempAssembly = GetCodeAssembly(declarations);
            if (tempAssembly.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                return new[] { new RazorToCSharpModel { Diagnostics = tempAssembly.Diagnostics } };
            }
            // add new assemblies to project refs
            var references = new List<MetadataReference>(_baseCompilation.References) { tempAssembly.Compilation.ToMetadataReference() };
            projectEngine = this.CreateRazorProjectEngine(references);
            var results = new List<RazorToCSharpModel>();
            // Iterates through c# code docs and converts to members of generated Razor project
            foreach (var declaration in declarations.Where(x => x.ProjectItem != null))
            {
                var codeDocument = projectEngine.Process(declaration.ProjectItem);
                var cSharpDocument = codeDocument.GetCSharpDocument();

                results.Add(new RazorToCSharpModel
                {
                    ProjectItem = declaration.ProjectItem,
                    Code = cSharpDocument.GeneratedCode,
                    Diagnostics = cSharpDocument.Diagnostics.Select(x => new CustomDiag(x))
                });
            }

            foreach (var declaration in declarations.Where(x => x.ProjectItem == null))
            {
                results.Add(new RazorToCSharpModel
                {
                    Code = declaration.Code,
                    Diagnostics = new List<CustomDiag>()
                });
            }
            return results;
        }

        private CodeAssemblyModel GetCodeAssembly(ICollection<RazorToCSharpModel> cSharpResults)
        {
            Console.WriteLine($"GetCodeAssembly: {string.Join("\r\n", cSharpResults.Select(x => x.Code))}");
           var cSharpParseOptions = new CSharpParseOptions(LanguageVersion.Preview);

            if (cSharpResults.Any(r => r.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)))
            {
                Console.WriteLine($"Razor compile error in GetCodeAssebly: {string.Join("\r\n", cSharpResults.SelectMany(r => r.Diagnostics).ToList())}");
                return new CodeAssemblyModel { Diagnostics = cSharpResults.SelectMany(r => r.Diagnostics).ToList() };
            }

            var syntaxTrees = new List<SyntaxTree>(cSharpResults.Count);
            foreach (var cSharpResult in cSharpResults)
            {
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(cSharpResult.Code, cSharpParseOptions));
            }

            var finalCompilation = _baseCompilation.AddSyntaxTrees(syntaxTrees);

            var compilationDiagnostics = finalCompilation.GetDiagnostics().Where(d => d.Severity > DiagnosticSeverity.Info);

            var result = new CodeAssemblyModel
            {
                Compilation = finalCompilation,
                Diagnostics = compilationDiagnostics.Select(diag => new CustomDiag(diag))
            };

            if (result.Diagnostics.All(x => x.Severity != DiagnosticSeverity.Error))
            {
                using var peStream = new MemoryStream();
                finalCompilation.Emit(peStream);

                result.AssemblyBytes = peStream.ToArray();

                return result;
            }

            return result;
        }
        public RazorProjectEngine CreateRazorProjectEngine(IReadOnlyList<MetadataReference> references)
        {
            Console.WriteLine("CreateRazorProjectEngine");
            return RazorProjectEngine.Create(_config, _fileSystem, builder =>
            {
                builder.SetRootNamespace(DefaultRootNamespace);
                builder.AddDefaultImports(RazorConstants.DefaultUsings);

                // Features that use Roslyn are mandatory for components
                CompilerFeatures.Register(builder);

                builder.Features.Add(new CompilationTagHelperFeature());
                builder.Features.Add(new DefaultMetadataReferenceFeature { References = references });
            });
        }

        private static RazorProjectItem CreateRazorProjectItem(string fileName, string fileContent)
        {
            Console.WriteLine($"CreateRazorProjectItem for {fileName}");
            var fullPath = WorkingDirectory + fileName;

            // File paths in Razor are always of the form '/a/b/c.razor'
            var filePath = fileName;
            if (!filePath.StartsWith('/'))
            {
                filePath = '/' + filePath;
            }

            fileContent = fileContent.Replace("\r", string.Empty);

            return new WebRazorProjectItem(
                WorkingDirectory,
                filePath,
                fullPath,
                fileName,
                FileKinds.Component,
                Encoding.UTF8.GetBytes(fileContent.TrimStart()));
        }
        public async Task InitAsync()
        {
            var basicReferenceAssemblyRoots = new[]
            {
                typeof(Console).Assembly, // System.Console
                typeof(Uri).Assembly, // System.Private.Uri
                typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Private.CoreLib
                typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
                typeof(IQueryable).Assembly, // System.Linq.Expressions
                typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
                typeof(HttpClient).Assembly, // System.Net.Http
                typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
                typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
            };

            var assemblyNames = basicReferenceAssemblyRoots
                .SelectMany(assembly => assembly.GetReferencedAssemblies().Concat(new[] { assembly.GetName() }))
                .Select(x => x.Name)
                .Distinct()
                .ToList();

            var assemblyStreams = await GetAssemblyStreams(httpClient, assemblyNames);

            Dictionary<string, PortableExecutableReference> allReferenceAssemblies = assemblyStreams.ToDictionary(a => a.Key, a => MetadataReference.CreateFromStream(a.Value));

            _references = allReferenceAssemblies
                .Where(a => basicReferenceAssemblyRoots
                    .Select(x => x.GetName().Name)
                    .Union(basicReferenceAssemblyRoots.SelectMany(y => y.GetReferencedAssemblies().Select(z => z.Name)))
                    .Any(n => n == a.Key))
                .Select(a => a.Value)
                .ToList();
            _appState.AssemblyReferences = _references.ToList();
            _appState.AssemblyNames = assemblyNames;
            _baseCompilation = CSharpCompilation.Create(
                "Output",
                Array.Empty<SyntaxTree>(),
                _references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, 
                    optimizationLevel: OptimizationLevel.Release,
                    concurrentBuild: false));
        }
        private static async Task<IDictionary<string, Stream>> GetAssemblyStreams(HttpClient httpClient, IEnumerable<string> assemblyNames)
        {
            var streams = new ConcurrentDictionary<string, Stream>();

            await Task.WhenAll(
                assemblyNames.Select(async assemblyName =>
                {
                    var result = await httpClient.GetAsync($"/_framework/{assemblyName}.dll");
                    result.EnsureSuccessStatusCode();
                    streams.TryAdd(assemblyName, await result.Content.ReadAsStreamAsync());
                }));

            return streams;
        }
    }
}
