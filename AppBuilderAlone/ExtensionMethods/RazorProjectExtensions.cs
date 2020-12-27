using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppBuilder.Client.Pages;
using AppBuilder.CompileRazor;
using AppBuilder.Shared;
using Microsoft.JSInterop;

namespace AppBuilder.Client.ExtensionMethods
{
    public static class RazorProjectExtensions
    {
        public static List<ProjectFile> PagifyMainComponent(this List<ProjectFile> codeFiles)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName);
            if (!mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = RazorConstants.MainComponentCodePrefix + mainComponent.Content;
            }

            return codeFiles;
        }

        public static List<ProjectFile> UnPagifyMainComponent(this List<ProjectFile> codeFiles, string originalContent)
        {
            var mainComponent = codeFiles.FirstOrDefault(x => x.Name == RazorConstants.DefaultComponentName);
            if (mainComponent.Content.Contains("@page"))
            {
                mainComponent.Content = originalContent;
            }

            return codeFiles;
        }

    }
}
