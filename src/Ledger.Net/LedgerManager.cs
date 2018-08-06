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

        protected async Task<byte[]> ReadMessageAsync()
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
            var indices = new[] { ((isSegwit ? (uint)49 : 44) | Constants.HardeningConstant) >> 0, (coinNumber | Constants.HardeningConstant) >> (int)account, (0 | Constants.HardeningConstant) >> 0, isChange ? 1 : (uint)0, index };

            byte[] addressIndicesData;

            using (var memoryStream = new MemoryStream())
            {
                var length = indices.Length;
                memoryStream.WriteByte((byte)indices.Length);
                for (var i = 0; i < indices.Length; i++)
                {
                    var bytes = UintToBytes(indices[i]);
                    memoryStream.Write(bytes, 0, bytes.Length);
                }
                addressIndicesData = memoryStream.ToArray();
            }

            var request = new BitcoinAppGetPublicKeyRequest(showDisplay, BitcoinAddressType.Segwit, addressIndicesData);

            var bitcoinAppGetPublicKeyResponse = await SendMessageAsync<BitcoinAppGetPublicKeyResponse,BitcoinAppGetPublicKeyRequest>(request);

            return bitcoinAppGetPublicKeyResponse.Address;
        }

        internal static byte[] UintToBytes(uint value)
        {
            return new byte[]
            {
                (byte)(value >> 24),
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value,
            };
        }

        public async Task<TResponse> SendMessageAsync<TResponse, TWrite>(TWrite message)
           where TResponse : ResponseBase
           where TWrite : RequestBase
        {
            await _SemaphoreSlim.WaitAsync();

            try
            {
                await WriteMessageAsync(message);
                var resultData = await ReadMessageAsync();
                return (TResponse)Activator.CreateInstance(typeof(TResponse), resultData);
            }
            finally
            {
                _SemaphoreSlim.Release();
            }
        }
        #endregion
    }
}
