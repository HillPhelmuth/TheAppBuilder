﻿using AppBuilder.Shared;
using Blazor.ModalDialog;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppBuilder.CompileConsole
{
    public class ConsoleCompile
    {
        private readonly List<MetadataReference> _references = new();
        private readonly AppState _appState;
        private readonly HttpClient _httpClient;

        public ConsoleCompile(AppState appState, HttpClient httpClient)
        {
            _appState = appState;
            _httpClient = httpClient;
        }

        public List<string> Logs { get; } = new();

        public async Task InitAsync()
        {
            if (_references.Count == 0)
            {
                _references.AddRange(await GetAssemblies());
            }
            
        }

        public async Task<string> CompileAndRun(ProjectFile[] csharpFiles)
        {
            Logs.Clear();
            var filesString = JsonSerializer.Serialize(csharpFiles);
            Console.WriteLine($"Files json: \r\n {filesString}");
            var assembly = await Compile(csharpFiles);

            return Run(assembly);
        }

        public async Task<Assembly> Compile(ProjectFile[] csharpFiles)
        {
            // Make sure the needed assembly references are available.
            await InitAsync();

            // Convert the C# files into syntax trees.
            var compilation = CSharpCompilation.Create("DynamicCode")
                .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
                .AddReferences(_references)
                .AddSyntaxTrees(csharpFiles.Select(x => CSharpSyntaxTree.ParseText(x.Content, new CSharpParseOptions(LanguageVersion.Preview)/*, x.Name*/)));

            var tempTreesJson = JsonSerializer.Serialize(compilation.SyntaxTrees);
            Console.WriteLine($"Syntax Trees:\r\n{tempTreesJson}");
            
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
                return null;
            }

            Logs.Add("");
            Logs.Add("Build succeeded.");

            // Reset stream to beginning.
            stream.Seek(0, SeekOrigin.Begin);

            // Load the newly created assembly into the current application domain.
            //var assembly = AppDomain.CurrentDomain.Load(stream.ToArray());
            var assembly = Assembly.Load(stream.ToArray());

            return assembly;
        }

        public string Run(Assembly assembly)
        {
            if (assembly == null)
                return $"Must have been some sort of fuck-up\r\n{string.Join(",", Logs)}";
            // Capture the Console outputs.
            var currentOut = Console.Out;
            var writer = new StringWriter();
            Console.SetOut(writer);

            var main = assembly.EntryPoint;

            var parameters = main.GetParameters().Length > 0
                ? new object[] { Array.Empty<string>() }
                : null;

            main.Invoke(null, parameters);
            var output = writer.ToString();
            Console.SetOut(currentOut);
            return output;
        }
        public async Task<List<MetadataReference>> GetAssemblies()
        {
            if (_appState.AssemblyReferences?.Count > 0) return _appState.AssemblyReferences;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic)
                .Select(x => x.GetName().Name)
                .Union(new[]
                {
                    // Add any required dll that are not referenced in the Blazor application
                    "System.Console",
                    "System.Linq"
                    //""
                })
                .Distinct()
                .Where(x => !string.IsNullOrWhiteSpace(x) && !_appState.AssemblyNames.Contains(x))
                .Select(x => $"_framework/{x}.dll");

            var references = new List<MetadataReference>();

            foreach (var assembly in assemblies)
            {
                // Download the assembly
                references.Add(
                    MetadataReference.CreateFromStream(
                        await _httpClient.GetStreamAsync(assembly)));
            }

            _appState.AssemblyReferences ??= references;
            references.AddRange(_appState.AssemblyReferences);
            return references;
        }
    }
}
