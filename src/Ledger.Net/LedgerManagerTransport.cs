using Device.Net;
using Ledger.Net.Requests;
using Ledger.Net.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManagerTransport : IHandlesRequest, IDisposable
    {
        #region Fields
        private readonly SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        private bool _IsDisposed;
        #endregion

        #region Public Properties
        public IDevice LedgerHidDevice { get; }
        #endregion

        #region Constructor
        public LedgerManagerTransport(IDevice ledgerHidDevice)
        {
            LedgerHidDevice = ledgerHidDevice;
        }
        #endregion

        #region Private Methods
        private async Task<IEnumerable<byte[]>> WriteRequestAndReadAsync<TRequest>(TRequest request) where TRequest : RequestBase
        {
            var responseData = new List<byte[]>();

            var apduChunks = request.ToAPDUChunks();

            for (var i = 0; i < apduChunks.Count; i++)
            {
                var apduCommandChunk = apduChunks[i];

                if (apduChunks.Count == 1)
                {
                    //There is only one chunk so use the argument from the request (e.g P1_SIGN)
                    apduCommandChunk[2] = request.Argument1;
                }
                else if (apduChunks.Count > 1)
                {
                    //There are multiple chunks so the assumption is that this is probably a transaction

                    if (i == 0)
                    {
                        //This is the first chunk of the transaction
                        apduCommandChunk[2] = Constants.P1_FIRST;
                    }
                    else if (i == (apduChunks.Count - 1))
                    {
                        //This is the last chunk of the transaction
                        apduCommandChunk[2] = Constants.P1_LAST;
                    }
                    else
                    {
                        //This is one of the middle chunks and there is more coming
                        apduCommandChunk[2] = Constants.P1_MORE;
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

                var responseDataChunk = await ReadAsync();

                responseData.Add(responseDataChunk);

                var returnCode = ResponseBase.GetReturnCode(responseDataChunk);

                if (returnCode != Constants.SuccessStatusCode)
                {
                    return responseData;
                }
            }
            return responseData;
        }

        private async Task<byte[]> ReadAsync()
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
                var responseDataChunks = await WriteRequestAndReadAsync(request);
                return (TResponse)Activator.CreateInstance(typeof(TResponse), responseDataChunks.Last());
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }
        #endregion

        #region Public Methods
        public async Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
    where TResponse : ResponseBase
    where TRequest : RequestBase
        {
            var response = await SendRequestAsync<TResponse>(request);

            //var data = string.Join(", ", response.Data.Select(b => b.ToString()));

            return response;
        }

        public void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            _SemaphoreSlim.Dispose();
            LedgerHidDevice?.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
