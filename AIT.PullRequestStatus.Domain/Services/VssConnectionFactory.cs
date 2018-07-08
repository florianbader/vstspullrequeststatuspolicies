using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIT.PullRequestStatus.Domain.Services
{
    public class VssConnectionFactory : IVssConnectionFactory
    {
        private static readonly IDictionary<string, VssConnection> _connections
            = new Dictionary<string, VssConnection>();

        private static readonly IDictionary<string, IVssServiceFactory> _factories
            = new Dictionary<string, IVssServiceFactory>();

        private readonly Uri _baseUrl;
        private readonly VssCredentials _credentials;

        public VssConnectionFactory()
        {
        }

        public VssConnectionFactory(Uri baseUrl, VssCredentials credentials)
        {
            _baseUrl = baseUrl;
            _credentials = credentials;
        }

        public IVssServiceFactory CreateFactory(Uri baseUrl, VssCredentials credentials) => _factories.ContainsKey(baseUrl.AbsoluteUri)
            ? _factories[baseUrl.AbsoluteUri]
            : (_factories[baseUrl.AbsoluteUri] = new VssConnectionFactory(baseUrl, credentials));

        public Task<T> GetClientAsync<T>() where T : VssHttpClientBase => CreateConnection().GetClientAsync<T>();

        private VssConnection CreateConnection(Uri baseUrl, VssCredentials credentials) => _connections.ContainsKey(baseUrl.AbsoluteUri)
            ? _connections[baseUrl.AbsoluteUri]
            : (_connections[baseUrl.AbsoluteUri] = new VssConnection(baseUrl, credentials));

        private VssConnection CreateConnection() => CreateConnection(_baseUrl, _credentials);
    }
}