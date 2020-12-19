using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AppStorageAPI
{
    public class AppBuilderStorage
    {
        private readonly string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");
        [FunctionName("GetProjects")]
        public async Task<IActionResult> GetProjects(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetProjects/{userName}")] HttpRequest req, string userName, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function GetProjects processed a request.");
            var container = GetContainer(userName);
            var result = new List<string>();
            await foreach (var blob in container.GetBlobsAsync())
            {
                var name = blob.Name.Replace(".zip", string.Empty);
                result.Add(name);
            }
            return new OkObjectResult(result);
        }
        [FunctionName("GetProjectFiles")]
        public async Task<IActionResult> GetProjectFiles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetProjectFiles/{userName}/{projectName}")] HttpRequest req, string userName, string projectName, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function GetProjectFiles processed a request.");
            var container = GetContainer(userName);
            var client = container.GetBlobClient($"{projectName}.zip");
            await using var zipFile = await client.OpenReadAsync();
            var result = new ProjectBlob{Name = projectName, CompressedFiles = await zipFile.ReadFully()};

            return new OkObjectResult(result);
        }
        [FunctionName("AddUserProject")]
        public async Task<IActionResult> AddUserProject(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "AddUserProject/{userName}")] HttpRequest req, string userName, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function AddUserProject processed a request.");
            var request = await req.ReadAsStringAsync();
            var projectBlob = JsonConvert.DeserializeObject<ProjectBlob>(request);
            try
            {
                await using var uploadFileStream = new MemoryStream(projectBlob.CompressedFiles);
                var container = GetContainer(userName);
                string fileName = $"{projectBlob.Name}.zip";
                var blob = container.GetBlobClient(fileName);
                var blobExists = await blob.ExistsAsync();
                await blob.UploadAsync(uploadFileStream, blobExists);
                if (blobExists)
                {
                    await blob.UploadAsync(uploadFileStream, true);
                }
                else
                {
                    await blob.UploadAsync(uploadFileStream);
                }
                
            }
            catch (Exception e)
            {
                return new BadRequestObjectResult($"Error saving file: {e.Message}\r\n{e.StackTrace}");
            }
            return new OkObjectResult($"{projectBlob.Name} was successfully uploaded");
        }
        //[FunctionName("UpdateUserProject")]
        //public async Task<IActionResult> UpdateUserProject(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "UpdateUserProject/{userName}")] HttpRequest req, string userName, ILogger log)
        //{
        //    log.LogInformation("C# HTTP trigger function processed a request.");

        //    string name = req.Query["name"];

        //    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //    dynamic data = JsonConvert.DeserializeObject(requestBody);
        //    name = name ?? data?.name;

        //    string responseMessage = string.IsNullOrEmpty(name)
        //        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        //        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        //    return new OkObjectResult(responseMessage);
        //}
        #region helper
        private BlobContainerClient GetContainer(string containerName = "sample")
        {
            string blobConnectionString = connectionString;
            string blobContainerName = containerName.ToValidContainerName();

            var container = new BlobContainerClient(blobConnectionString, blobContainerName);
            container.CreateIfNotExists();

            return container;
        }


        #endregion
    }
    public static class Extensions
    {
        public static async Task<byte[]> ReadFully(this Stream input)
        {
            await using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }
        public static string ToValidContainerName(this string str)
        {
            var sb = new StringBuilder();
            foreach (char c in str.Where(c => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '-'))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
