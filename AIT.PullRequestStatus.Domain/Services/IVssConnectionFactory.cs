using Microsoft.VisualStudio.Services.Common;
using System;

namespace AIT.PullRequestStatus.Domain.Services
{
    public interface IVssConnectionFactory : IVssServiceFactory
    {
        IVssServiceFactory CreateFactory(Uri baseUrl, VssCredentials credentials);
    }
}