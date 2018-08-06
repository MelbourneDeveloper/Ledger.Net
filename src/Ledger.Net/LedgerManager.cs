using Hid.Net;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ledger.Net
{
    public class LedgerManager
    {
        #region Fields
        private readonly IHidDevice _LedgerHidDevice;
        protected SemaphoreSlim _SemaphoreSlim = new SemaphoreSlim(1, 1);
        #endregion

        #region Constructor
        public LedgerManager(IHidDevice ledgerHidDevice)
        {
            _LedgerHidDevice = ledgerHidDevice;
        }
        #endregion

        #region Private Methods
        private async Task WriteMessageAsync<TWrite>(TWrite message) where TWrite : RequestBase
        {
            var packetIndex = 0;
            byte[] data = null;
            using (var memoryStream = new MemoryStream(message.ToAPDU()))
            {
                do
                {
                    data = GetDataPacket(memoryStream, packetIndex);
                    packetIndex++;
                    await _LedgerHidDevice.WriteAsync(data);
                } while (memoryStream.Position != memoryStream.Length);
            }
        }
        #endregion

        #region Private Static Methods
        public static byte[] ReadAllBytes(Stream stream, int totalByteCount)
        {
            var data = new byte[totalByteCount];
            var totalReadCount = 0;
            int readCount;
            do
            {
                totalReadCount += (readCount = stream.Read(data, totalReadCount, totalByteCount - totalReadCount));
            } while (readCount > 0 && totalReadCount < totalByteCount);
            return data;
        }

        private static byte[] GetDataPacket(Stream stream, int packetIndex)
        {
            using (var returnStream = new MemoryStream())
            {
                var position = (int)returnStream.Position;
                returnStream.WriteByte((Constants.DEFAULT_CHANNEL >> 8) & 0xff);
                returnStream.WriteByte(Constants.DEFAULT_CHANNEL & 0xff);
                returnStream.WriteByte(Constants.TAG_APDU);
                returnStream.WriteByte((byte)((packetIndex >> 8) & 0xff));
                returnStream.WriteByte((byte)(packetIndex & 0xff));

                if (packetIndex == 0)
                {
                    returnStream.WriteByte((byte)((stream.Length >> 8) & 0xff));
                    returnStream.WriteByte((byte)(stream.Length & 0xff));
                }

                var headerLength = (int)(returnStream.Position - position);
                var blockLength = Math.Min(Constants.LEDGER_HID_PACKET_SIZE - headerLength, (int)stream.Length - (int)stream.Position);

                var packetBytes = ReadAllBytes(stream, blockLength);

                returnStream.Write(packetBytes, 0, packetBytes.Length);

                while ((returnStream.Length % Constants.LEDGER_HID_PACKET_SIZE) != 0)
                {
                    returnStream.WriteByte(0);
                }

                return returnStream.ToArray();
            }
        }
        #endregion

        #region Public Methods
        public async Task<string> GetAddressAsync(string coinShortcut, uint coinNumber, bool isChange, uint index, bool showDisplay, AddressType addressType)
        {
            return null;
        }

        public async Task<TResponse> SendMessageAsync<TResponse, TWrite>(TWrite message)
           where TResponse : ResponseBase
           where TWrite : RequestBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {
                await WriteMessageAsync(message);
            }
            finally
            {
                _SemaphoreSlim.Release();
            }

            return default(TResponse);
        }
        #endregion
    }
}
