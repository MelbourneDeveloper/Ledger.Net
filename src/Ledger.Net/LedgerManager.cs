using Hardwarewallets.Net;
using Hardwarewallets.Net.AddressManagement;
using Hardwarewallets.Net.Model;
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
    public class LedgerManager : IAddressDeriver
    {
        #region Fields
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

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice) : this(ledgerHidDevice, null, null)
        {
        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility) : this(ledgerHidDevice, coinUtility, null)
        {

        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt)
        {
            ErrorPrompt = errorPrompt;

            LedgerHidDevice = ledgerHidDevice;

            CoinUtility = coinUtility ?? new DefaultCoinUtility();

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
            var commandIndex = 0;
            foreach (var apduCommandChunk in message.ToAPDUChunks())
            {
                if (commandIndex > 0)
                {
                    //Read intermediates response codes
                    ResponseBase response = new ResponseBase(await ReadResponseAsync());
                    if (!response.IsSuccess)
                    {
                        HandleErrorResponse(response);
                    }
                }

                var packetIndex = 0;
                byte[] data = null;
                using (var memoryStream = new MemoryStream(apduCommandChunk))
                {
                    do
                    {
                        data = Helpers.GetRequestDataPacket(memoryStream, packetIndex);
                        packetIndex++;
                        await LedgerHidDevice.WriteAsync(data);
                    } while (memoryStream.Position != memoryStream.Length);
                }

                commandIndex++;
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

        #endregion
    }
}
