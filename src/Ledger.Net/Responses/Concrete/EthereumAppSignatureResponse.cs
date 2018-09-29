using System.IO;

namespace Ledger.Net.Responses
{
    public class EthereumAppSignatureResponse : ResponseBase
    {
        public uint SignatureV { get; }

        public byte[] SignatureR { get; }

        public byte[] SignatureS { get; }

        public EthereumAppSignatureResponse(byte[] data) : base(data)
        {
            if (!IsSuccess)
            {
                return;
            }

            using (var memoryStream = new MemoryStream(data))
            {
                SignatureV = (uint)memoryStream.ReadByte();
                SignatureR = memoryStream.ReadAllBytes(32);
                SignatureS = memoryStream.ReadAllBytes(32);
            }
        }
    }
}
