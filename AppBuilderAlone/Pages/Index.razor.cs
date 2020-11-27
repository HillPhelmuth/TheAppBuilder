using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompileConsole;
using Microsoft.AspNetCore.Components;
using Shared;

namespace AppBuilderAlone.Pages
{
    public partial class Index
    {
        [Inject]
        private AppState AppState { get; set; }
        [Inject]
        private IDependencyResolver DependencyResolver { get; set; }
        protected override async Task OnInitializedAsync()
        {
            AppState.References = await DependencyResolver.GetAssemblies();
            await base.OnInitializedAsync();
        }
    }
}
