using Hid.Net;
using Ledger.Net.Requests;
using Ledger.Net.Requests.Helpers;
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
        public LedgerManager(IHidDevice ledgerHidDevice)
        {
            LedgerHidDevice = ledgerHidDevice;
        }
        #endregion

        #region Public Properties
        public IHidDevice LedgerHidDevice { get; }
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
                    data = GetRequestDataPacket(memoryStream, packetIndex);
                    packetIndex++;
                    await LedgerHidDevice.WriteAsync(data);
                } while (memoryStream.Position != memoryStream.Length);
            }
        }

        protected async Task<byte[]> ReadResponseAsync()
        {
            var remaining = 0;
            var packetIndex = 0;

            using (var response = new MemoryStream())
            {
                do
                {
                    var packetData = await LedgerHidDevice.ReadAsync();
                    var responseDataPacket = GetResponseDataPacket(packetData, packetIndex, ref remaining);
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

        #region Private Static Methods
        private static byte[] GetRequestDataPacket(Stream stream, int packetIndex)
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

                var packetBytes = stream.ReadAllBytes(blockLength);

                returnStream.Write(packetBytes, 0, packetBytes.Length);

                while ((returnStream.Length % Constants.LEDGER_HID_PACKET_SIZE) != 0)
                {
                    returnStream.WriteByte(0);
                }

                return returnStream.ToArray();
            }
        }

        protected byte[] GetResponseDataPacket(byte[] data, int packetIndex, ref int remaining)
        {
            using (var returnStream = new MemoryStream())
            {
                using (var input = new MemoryStream(data))
                {
                    var position = (int)input.Position;
                    var channel = input.ReadAllBytes(2);

                    int thirdByte = input.ReadByte();
                    if (thirdByte != Constants.TAG_APDU)
                    {
                        return null;
                    }

                    int fourthByte = input.ReadByte();
                    if (fourthByte != ((packetIndex >> 8) & 0xff))
                    {
                        return null;
                    }

                    int fifthByte = input.ReadByte();
                    if (fifthByte != (packetIndex & 0xff))
                    {
                        return null;
                    }

                    if (packetIndex == 0)
                    {
                        remaining = ((input.ReadByte()) << 8);
                        remaining |= input.ReadByte();
                    }

                    var headerSize = input.Position - position;
                    var blockSize = (int)Math.Min(remaining, Constants.LEDGER_HID_PACKET_SIZE - headerSize);

                    var commandPart = new byte[blockSize];

                    if (input.Read(commandPart, 0, commandPart.Length) != commandPart.Length)
                    {
                        return null;
                    }

                    returnStream.Write(commandPart, 0, commandPart.Length);

                    remaining -= blockSize;

                    return returnStream.ToArray();
                }
            }
        }
        #endregion

        #region Public Methods
        public async Task<string> GetAddressAsync(uint coinNumber, uint account, bool isChange, uint index, bool showDisplay, AddressType addressType, bool isSegwit)
        {
            byte[] data = PublicKeyHelpers.GetDerivationPathData(addressType, coinNumber, account, index, isChange, isSegwit);

            GetPublicKeyResponseBase response;
            if (addressType == AddressType.Ethereum)
            {
                response = await SendRequestAsync<EthereumAppGetPublicKeyResponse, EthereumAppGetPublicKeyRequest>(new EthereumAppGetPublicKeyRequest(showDisplay, false, data));
            }
            else
            {
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
