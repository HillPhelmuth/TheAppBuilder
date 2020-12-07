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

        public GithubClient(HttpClient client, IConfiguration config)
        {
            _client = client;
            _client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.raw");
            //_client.DefaultRequestHeaders.Add("Authorization",config.GetValue<string>("GithubToken"));
            //client.DefaultRequestHeaders.Add("Access-Control-Allow-Origin", "https://render.githubusercontent.com");
        }


        public async Task<Stream> CodeFromPublicRepo(string githubName, string repoName, string filepath)
        {
            //if (!filepath.Contains("."))
            //{
            //    return "Nope!, provide a file extension. I suggest '.cs'";
            //}
            var sw = new Stopwatch();
            sw.Start();
            var code = await _client.GetStreamAsync($"{baseUrl}/{githubName}/{repoName}/contents/{filepath}");
            sw.Stop();
            Console.WriteLine($"Retrieved code from Github in {sw.ElapsedMilliseconds}ms");
            return code;
        }

        //public async Task<Stream> DownloadGithubZip(string org, string repoName)
        //{
        //    var sw = new Stopwatch();
        //    sw.Start();
        //    if (string.IsNullOrWhiteSpace(org) || string.IsNullOrWhiteSpace(repoName)) return null;


        //   // return await _client.GetStreamAsync($"http://localhost:7071/api/DownloadGithubZip/{org}/{repoName}");

        //    //var resultString = await _client.GetStringAsync($"{baseUrl}/{org}/{repoName}");
        //    sw.Stop();
        //    var repoDetails = await _client.GetFromJsonAsync<RepoDetails>($"{baseUrl}/{org}/{repoName}");

        //    Console.WriteLine($"Download \r\n {repoDetails}\r\n from Github in {sw.ElapsedMilliseconds}");

        //    var result = await _client.GetStreamAsync($"{baseUrl}/{org}/{repoName}/zipball/master");
        //    return result;
        //}
    }
}
