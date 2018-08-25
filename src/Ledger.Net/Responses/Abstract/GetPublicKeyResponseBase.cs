using System.IO;

namespace Ledger.Net.Responses
{
    public abstract class GetPublicKeyResponseBase : ResponseBase
    {
        public string Address { get; }

        protected GetPublicKeyResponseBase(byte[] data) : base(data)
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
                Address = GetAddressFromStream(memoryStream, addressLength);
            }
        }

        protected abstract string GetAddressFromStream(Stream memoryStream, int addressLength);
    }
}
