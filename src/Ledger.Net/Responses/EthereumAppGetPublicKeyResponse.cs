using System;
using System.IO;
using System.Text;

namespace Ledger.Net.Requests
{
    public class EthereumAppGetPublicKeyResponse : ResponseBase
    {
        public string Address { get; }

        public EthereumAppGetPublicKeyResponse(byte[] data) : base(data)
        {
            if (!IsSuccess)
            {
                return;
            }

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
