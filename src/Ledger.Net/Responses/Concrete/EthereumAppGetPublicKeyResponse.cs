using System;
using System.Text;

namespace Ledger.Net.Responses
{
    public class EthereumAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public EthereumAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressString(byte[] addressData)
        {
            return "0x" + Encoding.ASCII.GetString(addressData).ToLower();
        }

        protected override string GetPublicKeyString(byte[] publicKeyData)
        {
            if (publicKeyData == null) throw new ArgumentNullException(nameof(publicKeyData));

            var sb = new StringBuilder();
            foreach (var @byte in publicKeyData)
            {
                sb.Append(@byte.ToString("X").ToLower());
            }

            return $"0x{sb}";
        }
    }
}