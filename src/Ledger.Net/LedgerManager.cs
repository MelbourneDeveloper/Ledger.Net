using Hid.Net;
using Ledger.Net.Exceptions;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManager
    {
        #region Fields
        private SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        #endregion

        #region Public Properties
        public ReturnCodePromptDelegate ReturnCodePrompt { get; }
        public int PromptRetryCount { get; set; } = 6;
        #endregion

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice) : this(ledgerHidDevice, null, null)
        {
        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility) : this(ledgerHidDevice, coinUtility, null)
        {

        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility, ReturnCodePromptDelegate returnCodePrompt)
        {
            ReturnCodePrompt = returnCodePrompt;

            LedgerHidDevice = ledgerHidDevice;
            CoinUtility = coinUtility;

            if (CoinUtility == null)
            {
                CoinUtility = new DefaultCoinUtility();
            }

            SetCoinNumber(0);
        }
        #endregion

        #region Public Properties
        public IHidDevice LedgerHidDevice { get; }
        public ICoinUtility CoinUtility { get; }
        public ICoinInfo CurrentCoin { get; private set; }
        #endregion

        #region Private Methods
        private async Task WriteRequestAsync<TWrite>(TWrite message) where TWrite : RequestBase
        {
            var packetIndex = 0;
            byte[] data = null;
            using (var memoryStream = new MemoryStream(message.ToAPDU()))
            {
                do
                {
                    data = Helpers.GetRequestDataPacket(memoryStream, packetIndex);
                    packetIndex++;
                    await LedgerHidDevice.WriteAsync(data);
                } while (memoryStream.Position != memoryStream.Length);
            }
        }

        private async Task<byte[]> ReadResponseAsync()
        {
            var remaining = 0;
            var packetIndex = 0;

            using (var response = new MemoryStream())
            {
                do
                {
                    var packetData = await LedgerHidDevice.ReadAsync();
                    var responseDataPacket = Helpers.GetResponseDataPacket(packetData, packetIndex, ref remaining);
                    packetIndex++;

                    if (responseDataPacket == null)
                    {
                        return null;
                    }

                    response.Write(responseDataPacket, 0, responseDataPacket.Length);

                } while (remaining != 0);

                return response.ToArray();
            }
        }

        private async Task<TResponse> SendRequestAsync<TResponse>(RequestBase request) where TResponse : ResponseBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {
                await WriteRequestAsync(request);
                var responseData = await ReadResponseAsync();
                return (TResponse)Activator.CreateInstance(typeof(TResponse), responseData);
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }

        private async Task<ResponseBase> CallAndPrompt<T>(Func<Task<T>> func, string memberName) where T : ResponseBase
        {
            for (var i = 0; i < PromptRetryCount; i++)
            {
                try
                {
                    var response = await func.Invoke();

                    if (response.IsSuccess)
                    {
                        return response;
                    }

                    if (ReturnCodePrompt == null)
                    {
                        HandleErrorResponse(response);
                    }
                    else
                    {
                        await ReturnCodePrompt(response.ReturnCode, null);
                    }
                }
                catch (Exception ex)
                {
                    await ReturnCodePrompt(null, ex);
                }
            }

            throw new TooManyPromptsException(PromptRetryCount, memberName);
        }
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

        private class asdasd
        {
            public ResponseBase rb { get; set; }
            public object ReturnValue { get; set; }
        }

        public async Task<string> GetAddressAsync(uint account, bool isChange, uint index, bool showDisplay)
        {
            var func = new Func<Task<GetPublicKeyResponseBase>>(async () =>
            {
                byte[] data = Helpers.GetDerivationPathData(CurrentCoin.App, CurrentCoin.CoinNumber, account, index, isChange, CurrentCoin.IsSegwit);

                GetPublicKeyResponseBase response;

                switch (CurrentCoin.App)
                {
                    case App.Ethereum:
                        response = await SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(showDisplay, false, data));
                        break;
                    case App.Bitcoin:
                        //TODO: Should we use the Coin's IsSegwit here?
                        response = await SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(showDisplay, BitcoinAddressType.Segwit, data));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return response;
            });

            var returnResponse = (GetPublicKeyResponseBase)await CallAndPrompt(func, nameof(GetAddressAsync));
            return returnResponse.Address;
        }

        public async Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
           where TResponse : ResponseBase
           where TRequest : RequestBase
        {
            return await SendRequestAsync<TResponse>(request);
        }
        #endregion
    }
}
