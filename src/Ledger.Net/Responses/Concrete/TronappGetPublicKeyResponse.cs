using System;
using System.Text;

namespace Ledger.Net.Responses
{
    public class TronAppGetPublicKeyResponse : GetPublicKeyResponseBase
    {
        public TronAppGetPublicKeyResponse(byte[] data) : base(data)
        {
        }

        protected override string GetAddressString(byte[] addressData)
        {
            return Encoding.ASCII.GetString(addressData);
        }

        protected override string GetPublicKeyString(byte[] publicKeyData)
        {
            if (publicKeyData == null) throw new ArgumentNullException(nameof(publicKeyData));

            var sb = new StringBuilder();
            foreach (var @byte in publicKeyData)
            {
                sb.Append(@byte.ToString("X2").ToLower());
            }

            return sb.ToString();
        }
    }
}
