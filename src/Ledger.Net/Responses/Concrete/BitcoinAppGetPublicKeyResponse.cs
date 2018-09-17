using System.IO;
using System.Text;

namespace Ledger.Net.Responses
{
    public class BitcoinAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public override string PublicKey => GetBitcoinAddressStringFromBytes(PublicKeyData);

        public BitcoinAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressFromStream(Stream memoryStream, int addressLength)
        {
            var bytes = memoryStream.ReadAllBytes(addressLength);
            return GetBitcoinAddressStringFromBytes(bytes);
        }

        private static string GetBitcoinAddressStringFromBytes(byte[] bytes)
        {
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
