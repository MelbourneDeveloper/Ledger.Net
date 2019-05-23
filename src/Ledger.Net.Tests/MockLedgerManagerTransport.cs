using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ledger.Net.Tests
{
    public class MockLedgerManagerTransport : IHandlesRequest
    {
        private static readonly byte[] NoAppLoadedResponseData = new byte[] { 105, 130 };
        private static readonly byte[] BitcoinPublicKeyResponseData = new byte[] { 78, 2, 80, 12, 244, 122, 65, 92, 56, 119, 215, 232, 56, 1, 60, 35, 147, 20, 185, 198, 91, 78, 62, 117, 162, 25, 19, 255, 233, 204, 129, 81, 102, 61, 238, 147, 158, 220, 54, 184, 123, 156, 123, 78, 122, 236, 27, 225, 87, 8, 80, 6, 155, 86, 68, 243, 33, 82, 110, 29, 19, 89, 145, 208, 235, 108, 34, 51, 78, 121, 68, 102, 117, 111, 71, 107, 56, 100, 86, 97, 103, 122, 97, 67, 118, 106, 72, 57, 122, 74, 111, 70, 88, 86, 100, 68, 86, 86, 53, 87, 109, 189, 71, 99, 138, 245, 30, 191, 210, 130, 5, 213, 10, 88, 174, 204, 194, 59, 13, 84, 21, 234, 220, 130, 247, 113, 225, 226, 235, 135, 102, 113, 45, 144, 0 };
        private static readonly byte[] TronTransactionResponseData = new byte[] { 150, 198, 46, 194, 23, 156, 245, 31, 8, 45, 124, 161, 175, 103, 46, 88, 223, 87, 34, 88, 243, 166, 165, 177, 67, 75, 168, 219, 23, 225, 154, 178, 51, 134, 133, 219, 252, 216, 138, 83, 103, 79, 64, 148, 71, 43, 125, 191, 49, 197, 67, 13, 239, 110, 15, 222, 168, 251, 64, 165, 250, 219, 229, 62, 0, 144, 0 };
        private static readonly byte[] GetCoinVersionResponseData = new byte[] { 0, 0, 0, 5, 1, 7, 66, 105, 116, 99, 111, 105, 110, 3, 66, 84, 67, 144, 0 };
        private static readonly byte[] EthereumAppGetIncorrectPublicKeyRequestData = new byte[] { 5, 128, 0, 0, 49, 128, 0, 0, 60, 128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public CurrentState CurrentState { get; set; }

        public Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
            where TResponse : ResponseBase
            where TRequest : RequestBase
        {
            ResponseBase response = null;

            if (request is EthereumAppGetPublicKeyRequest e && e.Data.ToList().SequenceEqual(EthereumAppGetIncorrectPublicKeyRequestData))
            {
                //This is a case where the request is Ethereum and the Purpose is Segwit.
                //TODO: This should hinge on CurrentState, not Request type
                throw new Exception();
            }

            switch (request)
            {
                case BitcoinAppGetPublicKeyRequest bitcoinAppGetPublicKeyRequest:

                    if (CurrentState == CurrentState.Dashboard)
                    {
                        response = new BitcoinAppGetPublicKeyResponse(NoAppLoadedResponseData);
                    }
                    else if (bitcoinAppGetPublicKeyRequest.Ins == Constants.BTCHIP_INS_GET_WALLET_PUBLIC_KEY)
                    {
                        response = new BitcoinAppGetPublicKeyResponse(BitcoinPublicKeyResponseData);
                    }
                    else
                    {
                        //TODO: Make this a real bitcoin address length
                        response = new BitcoinAppGetPublicKeyResponse(BitcoinPublicKeyResponseData);
                    }
                    break;

                case EthereumAppGetPublicKeyRequest ethereumAppGetPublicKeyRequest:
                    //TODO: Make this a real Ethereum address length
                    response = new EthereumAppGetPublicKeyResponse(BitcoinPublicKeyResponseData);
                    break;

                case TronAppSignatureRequest tronAppSignatureRequest:
                    response = new TronAppSignatureResponse(TronTransactionResponseData);
                    break;

                case TronAppGetPublicKeyRequest tronAppGetPublicKeyRequest:
                    //TODO: Make this a real Tron address length
                    response = new TronAppGetPublicKeyResponse(BitcoinPublicKeyResponseData);
                    break;

                case GetCoinVersionRequest getCoinVersionRequest:
                    response = new GetCoinVersionResponse(GetCoinVersionResponseData);
                    break;

                case EthereumAppSignatureRequest tronAppSignatureRequest:
                    response = new EthereumAppSignatureResponse(TronTransactionResponseData);
                    break;

                default:
                    throw new NotImplementedException();
            }




            return Task.FromResult((TResponse)response);
        }
    }
}