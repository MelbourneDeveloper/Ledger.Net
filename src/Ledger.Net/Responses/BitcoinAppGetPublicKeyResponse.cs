using System;
using System.IO;
using System.Text;

namespace Ledger.Net.Requests
{
    public class BitcoinAppGetPublicKeyResponse : ResponseBase
    {
        public string Address { get; }

        public BitcoinAppGetPublicKeyResponse(byte[] data) : base(data)
        {
            using (var memoryStream = new MemoryStream(data))
            {
                var publicKeyLength = memoryStream.ReadByte();
                var publicKeyData = memoryStream.ReadAllBytes(publicKeyLength);
                var addressLength = memoryStream.ReadByte();
                Address = Encoding.ASCII.GetString(memoryStream.ReadAllBytes(addressLength));
            }
        }
    }
}
