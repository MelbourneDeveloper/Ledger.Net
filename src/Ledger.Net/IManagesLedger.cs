using Ledger.Net.Responses;
using System;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public interface IManagesLedger : IDisposable
    {
        IHandlesRequest RequestHandler { get; }
        ICoinUtility CoinUtility { get; }
        ICoinInfo CurrentCoin { get; }
        Task SetCoinNumber();
        void SetCoinNumber(uint coinNumber);
        Task<ResponseBase> CallAndPrompt<T, T2>(Func<CallAndPromptArgs<T2>, Task<T>> func, CallAndPromptArgs<T2> state) where T : ResponseBase;
    }
}