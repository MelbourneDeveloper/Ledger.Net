using System.Threading.Tasks;
using Ledger.Net.Requests;
using Ledger.Net.Responses;

namespace Ledger.Net.Tests
{
    internal class MockLedgerManagerTransport : IHandlesRequest
    {
        public Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
            where TResponse : ResponseBase
            where TRequest : RequestBase
        {
            throw new System.NotImplementedException();
        }
    }
}