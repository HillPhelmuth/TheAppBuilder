using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppBuilder.Shared.StaticAuth.Interfaces
{
    public interface ICustomAuthenticationStateProvider
    {
        Task<AuthenticationState> GetAuthenticationStateAsync();
    }
}
