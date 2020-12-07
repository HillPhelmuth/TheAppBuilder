using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace AppBuilder.Client.StaticCustomAuth.Interfaces
{
    public interface ICustomAuthenticationStateProvider
    {
        Task<AuthenticationState> GetAuthenticationStateAsync();
    }
}
