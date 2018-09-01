using System.IO;

namespace Ledger.Net.Responses
{
    public class EthereumAppSignTransactionResponse : ResponseBase
    {
        // https://github.com/LedgerHQ/ledger-app-eth/blob/master/doc/ethapp.asc
        // See "SIGN ETH TRANSACTION" section for the output.
        // This is most likely correct, however correct transaction signing appearing on the device is still needed.

        public uint SignatureV { get; }

        public byte[] SignatureR { get; }

        public byte[] SignatureS { get; }

        public EthereumAppSignTransactionResponse(byte[] data) : base(data)
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
