using System.IO;
using System.Text;

namespace Ledger.Net.Responses
{
    public class EthereumAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public override string PublicKey
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var @byte in PublicKeyData)
                {
                    sb.Append(@byte.ToString("X").ToLower());
                }

                return $"0x{sb.ToString()}";
            }
        }

        public EthereumAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressFromStream(Stream memoryStream, int addressLength)
        {
            var bytes = memoryStream.ReadAllBytes(addressLength);
            var retVal = GetEthformattedStringFromBytes(bytes);
            return retVal;
        }

        private static string GetEthformattedStringFromBytes(byte[] bytes)
        {
            return "0x" + Encoding.ASCII.GetString(bytes).ToLower();
        }
    }
}
