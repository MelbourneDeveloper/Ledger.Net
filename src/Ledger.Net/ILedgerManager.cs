using Hardwarewallets.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public interface ILedgerManager : IAddressDeriver, IDisposable
    {
        string DeviceId { get; }
        ICoinUtility CoinUtility { get; }
        ICoinInfo CurrentCoin { get; }
        ErrorPromptDelegate ErrorPrompt { get; set; }
        int PromptRetryCount { get; set; }

        Task<ResponseBase> CallAndPrompt<T, T2>(Func<CallAndPromptArgs<T2>, Task<T>> func, CallAndPromptArgs<T2> state) where T : ResponseBase;
        Task<string> GetAddressAsync(uint account, bool isChange, uint index, bool showDisplay);
        Task<string> GetAddressAsync(uint account, uint index);
        Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
            where TResponse : ResponseBase
            where TRequest : RequestBase;
        Task SetCoinNumber();
        void SetCoinNumber(uint coinNumber);
        Task ReconnectAsync();
    }
}