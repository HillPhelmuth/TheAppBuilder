using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.CodeAnalysis;
using Microsoft.JSInterop;

namespace AppBuilder.Shared
{
    public interface IDependencyResolver
    {
        Task<List<MetadataReference>> GetAssemblies();
        Task<IEnumerable<MetadataReference>> GetRazorAssemblies();
    }

    public class DependencyResolver : IDependencyResolver
    {
        private readonly HttpClient _http;

        public DependencyResolver(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<MetadataReference>> GetAssemblies()
        {
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
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => $"_framework/{x}.dll");

            var references = new List<MetadataReference>();

            foreach (var assembly in assemblies)
            {
                // Download the assembly
                references.Add(
                    MetadataReference.CreateFromStream(
                        await _http.GetStreamAsync(assembly)));
            }

            return references;
        }
        public async Task<IEnumerable<MetadataReference>> GetRazorAssemblies(/*HttpClient httpClient*/)
        {
            HttpClient httpClient = _http;
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
                .Where(x => !string.IsNullOrWhiteSpace(x));
            var basicReferenceAssemblyRoots = new[]
            {
                typeof(AssemblyTargetedPatchBandAttribute).Assembly, // System.Runtime
                typeof(NavLink).Assembly, // Microsoft.AspNetCore.Components.Web
                typeof(IQueryable).Assembly, // System.Linq
                typeof(HttpClientJsonExtensions).Assembly, // System.Net.Http.Json
                typeof(HttpClient).Assembly, // System.Net.Http
                typeof(Uri).Assembly, // System.Private.Uri
                typeof(IJSRuntime).Assembly, // Microsoft.JSInterop
                typeof(RequiredAttribute).Assembly, // System.ComponentModel.Annotations
            };

            var assemblyNames = basicReferenceAssemblyRoots
                .SelectMany(assembly => assembly.GetReferencedAssemblies().Concat(new[] { assembly.GetName() }))
                .Select(x => x.Name)
                .Union(assemblies)
                .Distinct()
                .Select(x => $"_framework/{x}.dll")
                .ToList();

            var assemblyStreams = await GetAssemblyStreams(httpClient, assemblyNames);

            Dictionary<string, PortableExecutableReference> allReferenceAssemblies = assemblyStreams.ToDictionary(a => a.Key, a => MetadataReference.CreateFromStream(a.Value));

            return allReferenceAssemblies
                .Where(a => basicReferenceAssemblyRoots
                    .Select(x => x.GetName().Name)
                    .Union(basicReferenceAssemblyRoots.SelectMany(y => y.GetReferencedAssemblies().Select(z => z.Name)))
                    .Any(n => n == a.Key))
                .Select(a => a.Value);
        }
        private static async Task<IDictionary<string, Stream>> GetAssemblyStreams(HttpClient httpClient, IEnumerable<string> assemblyNames)
        {
            var streams = new ConcurrentDictionary<string, Stream>();

            await Task.WhenAll(
                assemblyNames.Select(async assemblyName =>
                {
                    var result = await httpClient.GetAsync($"{assemblyName}");

                    result.EnsureSuccessStatusCode();

                    streams.TryAdd(assemblyName, await result.Content.ReadAsStreamAsync());
                }));

            return streams;
        }
    }
}
