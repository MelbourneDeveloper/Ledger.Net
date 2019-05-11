using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using Hardwarewallets.Net.Model;
using Ledger.Net.Exceptions;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public abstract class LedgerManagerBase : IAddressDeriver, ILedgerManager
    {
        #region Fields
        private bool _IsDisposed;

        private readonly SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly Func<CallAndPromptArgs<GetAddressArgs>, Task<GetPublicKeyResponseBase>> _GetAddressFunc = async s =>
        {
            var lm = s.LedgerManager;

            var data = Helpers.GetDerivationPathData(s.Args.AddressPath);

            GetPublicKeyResponseBase response;

            switch (lm.CurrentCoin.App)
            {
                case App.Ethereum:
                    response = await lm.SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(s.Args.ShowDisplay, false, data));
                    break;
                case App.BitcoinGold:
                case App.Bitcoin:
                    //TODO: Should we use the Coin's IsSegwit here?
                    response = await lm.SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(s.Args.ShowDisplay, BitcoinAddressType.Segwit, data));
                    break;
                case App.Tron:
                    response = await lm.SendRequestAsync<TronAppGetPublicKeyResponse, TronAppGetPublicKeyRequest>(new TronAppGetPublicKeyRequest(s.Args.ShowDisplay, data));
                    break;
                default:
                    throw new NotImplementedException();
            }

            return response;
        };
        #endregion

        #region Public Properties
        public ErrorPromptDelegate ErrorPrompt { get; set; }
        public int PromptRetryCount { get; set; } = 6;
        #endregion

        #region Public Abstract Properties
        public abstract string DeviceId { get; }
        #endregion

        #region Constructor
        public LedgerManagerBase() : this(null, null)
        {
        }

        public LedgerManagerBase(ICoinUtility coinUtility) : this(coinUtility, null)
        {

        }

        public LedgerManagerBase(ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            ErrorPrompt = errorPrompt;

            CoinUtility = coinUtility ?? new DefaultCoinUtility();

            SetCoinNumber(0);
        }
        #endregion

        #region Public Properties
        public ICoinUtility CoinUtility { get; }
        public ICoinInfo CurrentCoin { get; private set; }
        #endregion

        #region Private Methods
        private void CheckForDisposed()
        {
            if (_IsDisposed) throw new ObjectDisposedException($"The {nameof(LedgerManager)} is Disposed. It can not longer function.", nameof(LedgerManager));
        }

        private async Task<TResponse> SendRequestAsync<TResponse>(RequestBase request) where TResponse : ResponseBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {
                var responseDataChunks = await WriteRequestAndReadAsync(request);
                return (TResponse)Activator.CreateInstance(typeof(TResponse), responseDataChunks.Last());
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }
        #endregion

        #region Protected Abstract Methods
        protected abstract Task<IEnumerable<byte[]>> WriteRequestAndReadAsync<TRequest>(TRequest request) where TRequest : RequestBase;
        protected abstract Task<byte[]> ReadAsync();
        #endregion  

        #region Private Static Methods
        private static void HandleErrorResponse(ResponseBase response)
        {
            switch (response.ReturnCode)
            {
                case Constants.InstructionNotSupportedStatusCode:
                    throw new InstructionNotSupportedException(response.Data);
                case Constants.SecurityNotValidStatusCode:
                    throw new SecurityException(response.Data);
                case Constants.IncorrectLengthStatusCode:
                    throw new IncorrectLengthException(response.Data);
                default:
                    throw new Exception(response.StatusMessage);
            }
        }
        #endregion

        #region Public Methods

        public void SetCoinNumber(uint coinNumber)
        {
            CurrentCoin = CoinUtility.GetCoinInfo(coinNumber);
        }

        /// <summary>
        /// This will set the coin based on the currently open app. Note: this only currently works with Bitcoin based Ledger apps.
        /// </summary>
        public async Task SetCoinNumber()
        {
            var getCoinVersionResponse = await SendRequestAsync<GetCoinVersionResponse, GetCoinVersionRequest>(new GetCoinVersionRequest());

            if (!getCoinVersionResponse.IsSuccess)
            {
                HandleErrorResponse(getCoinVersionResponse);
            }

            CurrentCoin = CoinUtility.GetCoinInfo(getCoinVersionResponse.ShortCoinName);
        }

        public async Task<string> GetAddressAsync(uint account, uint index)
        {
            return await GetAddressAsync(account, false, index, false);
        }

        public Task<string> GetAddressAsync(uint account, bool isChange, uint index, bool showDisplay)
        {
            return GetAddressAsync(new BIP44AddressPath(CurrentCoin.IsSegwit, CurrentCoin.CoinNumber, account, isChange, index), false, showDisplay);
        }

        public async Task<string> GetAddressAsync(IAddressPath addressPath, bool isPublicKey, bool display)
        {
            CheckForDisposed();

            var returnResponse = (GetPublicKeyResponseBase)await CallAndPrompt(_GetAddressFunc,
                new CallAndPromptArgs<GetAddressArgs>
                {
                    LedgerManager = this,
                    MemberName = nameof(GetAddressAsync),
                    Args = new GetAddressArgs(addressPath, display)
                });

            return isPublicKey ? returnResponse.PublicKey : returnResponse.Address;
        }

        public async Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
           where TResponse : ResponseBase
           where TRequest : RequestBase
        {
            return await SendRequestAsync<TResponse>(request);
        }

        public async Task<ResponseBase> CallAndPrompt<T, T2>(Func<CallAndPromptArgs<T2>, Task<T>> func, CallAndPromptArgs<T2> state) where T : ResponseBase
        {
            for (var i = 0; i < PromptRetryCount; i++)
            {
                CheckForDisposed();

                try
                {
                    var response = await func.Invoke(state);

                    if (response.IsSuccess)
                    {
                        return response;
                    }

                    if (ErrorPrompt == null)
                    {
                        HandleErrorResponse(response);
                    }
                    else
                    {
                        await ErrorPrompt(response.ReturnCode, null, state.MemberName);
                    }
                }
                catch (Exception ex)
                {
                    if (ErrorPrompt == null)
                    {
                        throw;
                    }

                    await ErrorPrompt(null, ex, state.MemberName);
                }
            }

            throw new TooManyPromptsException(PromptRetryCount, state.MemberName);
        }

        public virtual void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            _SemaphoreSlim.Dispose();
            GC.SuppressFinalize(this);
        }

        ~LedgerManagerBase()
        {
            Dispose();
        }
        #endregion

        #region Public Abstract Methods
        public abstract Task ReconnectAsync();
        #endregion
    }
}
