using Device.Net;
using Ledger.Net.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManager : LedgerManagerBase
    {
        #region Fields
        private bool _IsDisposed;
        #endregion

        #region Public Properties
        public IDevice LedgerHidDevice { get; }
        public override string DeviceId => LedgerHidDevice?.DeviceId;
        #endregion

        #region Constructor
        public LedgerManager(IDevice ledgerHidDevice) : this(ledgerHidDevice, null, null)
        {
        }

        public LedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility) : this(ledgerHidDevice, coinUtility, null)
        {

        }

        public LedgerManager(IDevice ledgerHidDevice, ICoinUtility coinUtility, ErrorPromptDelegate errorPrompt) : base(coinUtility, errorPrompt)
        {
            LedgerHidDevice = ledgerHidDevice;
        }
        #endregion


        #region Protected Overrides
        protected override async Task<IEnumerable<byte[]>> WriteRequestAndReadAsync<TRequest>(TRequest request) 
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

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

        protected override async Task<byte[]> ReadAsync()
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
        #endregion


        #region Public Methods
        public override void Dispose()
        {
            if (_IsDisposed) return;
            _IsDisposed = true;

            LedgerHidDevice?.Dispose();

            base.Dispose();

            GC.SuppressFinalize(this);
        }

        public override Task ReconnectAsync()
        {
            return LedgerHidDevice.InitializeAsync();
        }

        ~LedgerManager()
        {
            Dispose();
        }
        #endregion
    }
}
