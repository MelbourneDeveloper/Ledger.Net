using System.IO;
using System.Text;

namespace Ledger.Net.Responses
{
    public class EthereumAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public EthereumAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressFromStream(Stream memoryStream, int addressLength)
        {
            return "0x" + Encoding.ASCII.GetString(memoryStream.ReadAllBytes(addressLength)).ToLower();
        }
    }
}
