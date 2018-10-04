using System.IO;

namespace Ledger.Net.Responses
{
    public abstract class GetPublicKeyResponseBase : ResponseBase
    {
        public string Address { get; }

        public byte[] AddressData { get; }

        public string PublicKey { get; }

        public byte[] PublicKeyData { get; }

        public byte[] ChainCodeData { get; }

        protected GetPublicKeyResponseBase(byte[] data) : base(data)
        {
            if (!IsSuccess)
            {
                return;
            }

            using (var memoryStream = new MemoryStream(data))
            {
                var publicKeyLength = memoryStream.ReadByte();
                PublicKeyData = memoryStream.ReadAllBytes(publicKeyLength);
                PublicKey = GetPublicKeyString(PublicKeyData);

                var addressLength = memoryStream.ReadByte();
                AddressData = memoryStream.ReadAllBytes(addressLength);
                Address = GetAddressString(AddressData);

                ChainCodeData = memoryStream.ReadAllBytes(32);
            }
        }

        protected abstract string GetPublicKeyString(byte[] publicKeyData);

        protected abstract string GetAddressString(byte[] addressData);
    }
}
