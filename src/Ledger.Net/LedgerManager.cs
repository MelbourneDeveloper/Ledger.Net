using Hid.Net;
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
        protected SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        #endregion

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice) : this(ledgerHidDevice, null)
        {
        }

        public LedgerManager(IHidDevice ledgerHidDevice, ICoinUtility coinUtility)
        {
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
            var getCoinVersionRequest = await SendRequestAsync<GetCoinVersionResponse, GetCoinVersionRequest>(new GetCoinVersionRequest());
            CurrentCoin = CoinUtility.GetCoinInfo(getCoinVersionRequest.ShortCoinName);
        }

        public async Task<string> GetAddressAsync(uint account, uint index)
        {
            return await GetAddressAsync(account, false, index, false);
        }

        public async Task<EthereumAppSignTransactionResponse> EthSignTransactionAsync(string hexNonce, string hexGasPrice, string hexGasLimit, string addressTo, string hexValue, string data, string hexChainId)
        {
            byte[] derivationData = Helpers.GetDerivationPathData(CurrentCoin.App, CurrentCoin.CoinNumber, 0, 0, false, CurrentCoin.IsSegwit);
            byte[] firstBlock = EthHelpers.GetTransactionData(derivationData, hexNonce, hexGasPrice, hexGasLimit, addressTo, hexValue, data, hexChainId);
            byte[] secondBlock = EthHelpers.GetTransactionData(null, hexNonce, hexGasPrice, hexGasLimit, addressTo, hexValue, data, hexChainId);

            EthereumAppSignTransactionRequest firstRequest = new EthereumAppSignTransactionRequest(true, firstBlock);
            EthereumAppSignTransactionRequest secondRequest = new EthereumAppSignTransactionRequest(false, secondBlock);

            await SendRequestAsync<EthereumAppSignTransactionResponse, EthereumAppSignTransactionRequest>(firstRequest);
            return await SendRequestAsync<EthereumAppSignTransactionResponse, EthereumAppSignTransactionRequest>(secondRequest);
        }

        public async Task<string> GetAddressAsync(uint account, bool isChange, uint index, bool showDisplay)
        {
            byte[] data = Helpers.GetDerivationPathData(CurrentCoin.App, CurrentCoin.CoinNumber, account, index, isChange, CurrentCoin.IsSegwit);

            GetPublicKeyResponseBase response;
            if (CurrentCoin.App == App.Ethereum)
            {
                response = await SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(showDisplay, false, data));
            }
            else
            {
                //TODO: Should we use the Coin's IsSegwit here?
                response = await SendRequestAsync<BitcoinAppGetPublicKeyResponse, BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(showDisplay, BitcoinAddressType.Segwit, data));
            }

            if (!response.IsSuccess)
            {
                throw new Exception(response.StatusMessage);
            }

            return response.Address;
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
