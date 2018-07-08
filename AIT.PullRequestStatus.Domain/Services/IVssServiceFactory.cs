using Microsoft.VisualStudio.Services.WebApi;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.Services
{
    public interface IVssServiceFactory
    {
        Task<T> GetClientAsync<T>() where T : VssHttpClientBase;
    }
}