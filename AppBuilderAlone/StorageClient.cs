using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AppBuilder.Shared;

namespace AppBuilder.Client
{
    public class StorageClient
    {
        private readonly HttpClient _client;
        private readonly ZipService _zipService;
        private readonly string baseUrl = "https://appbuilderapifunctions.azurewebsites.net/api"; /*@"http://localhost:7071/api";*/

        public StorageClient(HttpClient client, ZipService zipService)
        {
            _client = client;
            _zipService = zipService;
        }

        public async Task<List<string>> GetUserProjects(string username)
        {
            var projectNames = await _client.GetFromJsonAsync<List<string>>($"{baseUrl}/GetProjects/{username}");
            return projectNames;
        }

        public async Task<UserProject> GetProject(string username, string projectName)
        {
            var projectBlob =
                await _client.GetFromJsonAsync<ProjectBlob>($"{baseUrl}/GetProjectFiles/{username}/{projectName}");
            var projectFiles = await _zipService.ExtractFiles(new MemoryStream(projectBlob.CompressedFiles));
            return new UserProject {Name = projectBlob.Name, Files = projectFiles};
        }

        public async Task<string> UploadProject(string username, UserProject project)
        {
            var compressedFiles = await _zipService.ZipUpFiles(project.Files);
            var projectBlob = new ProjectBlob {Name = project.Name, CompressedFiles = compressedFiles};
            var response = await _client.PostAsJsonAsync($"{baseUrl}/AddUserProject/{username}", projectBlob);
            return $"API Response\r\n{response.ReasonPhrase}";
        }
    }
}
