using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AppBuilder.Shared;
using Microsoft.Extensions.Configuration;

namespace AppBuilder.Client
{
    public class GithubClient
    {
        private readonly HttpClient _client;
        private readonly string baseUrl = @"https://api.github.com/repos";

        public GithubClient(HttpClient client)
        {
            _client = client;
            _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
        }


        public async Task<Stream> CodeFromPublicRepo(string githubName, string repoName, string filepath)
        {
            
            var sw = new Stopwatch();
            sw.Start();
            var code = await _client.GetStreamAsync($"{baseUrl}/{githubName}/{repoName}/contents/{filepath}");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }

    }
}
