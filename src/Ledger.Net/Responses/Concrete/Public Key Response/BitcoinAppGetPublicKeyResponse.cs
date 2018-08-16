using System.IO;
using System.Text;

namespace Ledger.Net.Responses
{
    public class BitcoinAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public BitcoinAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressFromStream(Stream memoryStream, int addressLength)
        {
            return Encoding.ASCII.GetString(memoryStream.ReadAllBytes(addressLength));
        }
    }
}
