using Hid.Net;
using Ledger.Net.Requests;
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
        private async Task WriteRequestAsync<TWrite>(TWrite message) where TWrite : RequestBase
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

        protected async Task<byte[]> ReadResponseAsync()
        {
            var response = new MemoryStream();
            var remaining = 0;
            var sequenceIdx = 0;

            do
            {
                var result = await _LedgerHidDevice.ReadAsync();
                var commandPart = UnwrapReponseAPDU(result, ref sequenceIdx, ref remaining);
                if (commandPart == null)
                    return null;
                response.Write(commandPart, 0, commandPart.Length);
            } while (remaining != 0);

            return response.ToArray();
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

        protected byte[] UnwrapReponseAPDU(byte[] data, ref int sequenceIdx, ref int remaining)
        {
            var output = new MemoryStream();
            var input = new MemoryStream(data);
            var position = (int)input.Position;
            var channel = ReadAllBytes(input, 2);
            if (input.ReadByte() != Constants.TAG_APDU)
                return null;
            if (input.ReadByte() != ((sequenceIdx >> 8) & 0xff))
                return null;
            if (input.ReadByte() != (sequenceIdx & 0xff))
                return null;

            if (sequenceIdx == 0)
            {
                remaining = ((input.ReadByte()) << 8);
                remaining |= input.ReadByte();
            }
            sequenceIdx++;
            var headerSize = input.Position - position;
            var blockSize = (int)Math.Min(remaining, Constants.LEDGER_HID_PACKET_SIZE - headerSize);

            var commandPart = new byte[blockSize];
            if (input.Read(commandPart, 0, commandPart.Length) != commandPart.Length)
                return null;
            output.Write(commandPart, 0, commandPart.Length);
            remaining -= blockSize;
            return output.ToArray();
        }

        #endregion

        #region Public Methods
        public async Task<string> GetAddressAsync(uint coinNumber, uint account, bool isChange, uint index, bool showDisplay, AddressType addressType)
        {
            var isSegwit = true;
            var indices = new[] { ((isSegwit ? (uint)49 : 44) | Constants.HARDENING_CONSTANT) >> 0, (coinNumber | Constants.HARDENING_CONSTANT) >> (int)account, (0 | Constants.HARDENING_CONSTANT) >> 0, isChange ? 1 : (uint)0, index };

            byte[] addressIndicesData;

            using (var memoryStream = new MemoryStream())
            {
                memoryStream.WriteByte((byte)indices.Length);
                for (var i = 0; i < indices.Length; i++)
                {
                    var data = indices[i].ToBytes();
                    memoryStream.Write(data, 0, data.Length);
                }
                addressIndicesData = memoryStream.ToArray();
            }

            var bitcoinAppGetPublicKeyResponse = await SendRequestAsync<BitcoinAppGetPublicKeyResponse,BitcoinAppGetPublicKeyRequest>(new BitcoinAppGetPublicKeyRequest(showDisplay, BitcoinAddressType.Segwit, addressIndicesData));

            return bitcoinAppGetPublicKeyResponse.Address;
        }

        public async Task<TResponse> SendRequestAsync<TResponse, TRequest>(TRequest request)
           where TResponse : ResponseBase
           where TRequest : RequestBase
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
    }
}
