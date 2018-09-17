using System.IO;
using System.Text;

namespace Ledger.Net.Responses
{
    public class BitcoinAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public override string PublicKey
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var @byte in PublicKeyData)
                {
                    sb.Append(@byte.ToString("X2").ToLower());
                }

                return sb.ToString();
            }
        }

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
