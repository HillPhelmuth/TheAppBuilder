using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppBuilder.Shared
{
    public class ZipService
    {
        public async Task<List<ProjectFile>> ExtractFiles(Stream fileData)
        {
            var sw = new Stopwatch();
            sw.Start();
            await using var ms = new MemoryStream();
            await fileData.CopyToAsync(ms);
            using var archive = new ZipArchive(ms, ZipArchiveMode.Update);
            CleanUpSolution(archive);
            var entries = new List<ProjectFile>();

            foreach (var entry in archive.Entries.Where(x => x.FullName.EndsWith(".cs") || x.FullName.EndsWith(".razor")))
            {
                await using var fileStream = entry.Open();
                var fileBytes = await fileStream.ReadFully();
                var content = Encoding.UTF8.GetString(fileBytes);
                entries.Add(new ProjectFile { Name = entry.FullName.GetFileNameOnly(), Content = content, FileType = entry.FullName.EndsWith(".cs") ? FileType.Class : FileType.Razor });
            }
            sw.Stop();
            Console.WriteLine($"ExtractFiles in {sw.ElapsedMilliseconds}");
            return entries;
        }

        public Task<byte[]> ZipUpFiles(List<ProjectFile> projectFiles)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
            {
                foreach (var file in projectFiles)
                {
                    var demoFile = archive.CreateEntry(file.Name);

                    using var entryStream = demoFile.Open();
                    using var streamWriter = new StreamWriter(entryStream);
                    streamWriter.Write(file.Content);
                }

            }

            //using (var fileStream = new FileStream(@"C:\Temp\test.zip", FileMode.Create))
            //{
            //    memoryStream.Seek(0, SeekOrigin.Begin);
            //    memoryStream.CopyTo(fileStream);
            //}
            return Task.FromResult(memoryStream.ToArray());
        }

        private static void CleanUpSolution(ZipArchive archive)
        {
            var foldersToDelete = new[] { "bin", "obj", ".vs", ".git", ".vscode" }
                .Select(x => $"/{x}/")
                .ToArray();

            var entriesToDelete = archive.Entries
                .Where(entry => foldersToDelete.Any(folder => entry.FullName.Contains(folder)))
                .ToList();

            foreach (var entry in entriesToDelete)
            {
                archive
                    .GetEntry(entry.FullName)
                    .Delete();
            }
        }
    }

    public static class StreamExtension
    {
        public static async Task<byte[]> ReadFully(this Stream input)
        {
            await using var ms = new MemoryStream();
            await input.CopyToAsync(ms);
            return ms.ToArray();
        }

        public static string GetFileNameOnly(this string path)
        {
            if (!path.Contains('/')) return path;
            return path.Split('/').Reverse().FirstOrDefault(x => x.EndsWith(".cs") || x.EndsWith(".razor"));
        }
    }
}
