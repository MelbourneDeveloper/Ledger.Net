using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public interface IHandlesRequest
    {
        Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
            where TResponse : ResponseBase
            where TRequest : RequestBase;
    }
}