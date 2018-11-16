using System;

namespace Ledger.Net.Requests
{
    public class EthereumAppSignatureRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => 0;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => SignTransaction ? Constants.ETHEREUM_SIGN_TX : Constants.ETHEREUM_SIGN_MESSAGE;
        public override bool Chunked => Data.Length > Constants.ETHEREUM_MAX_CHUNK_SIZE;
        #endregion

        #region Public Properties
        public bool SignTransaction { get; }
        #endregion

        #region Constructor
        public EthereumAppSignatureRequest(bool signTransaction, byte[] data) : base(data)
        {
            SignTransaction = signTransaction;
        }
        #endregion

        #region Protected Overrides
        protected override byte[] GetApduChain(ref int offset)
        {
            var chunkSize = offset + Constants.ETHEREUM_MAX_CHUNK_SIZE > Data.Length ? Data.Length - offset : Constants.ETHEREUM_MAX_CHUNK_SIZE;
            byte[] buffer = new byte[5 + chunkSize];
            buffer[0] = Cla;
            buffer[1] = Ins;
            buffer[2] = (byte)(offset == 0 ? 0x00 : 0x80);
            buffer[3] = Argument2;
            buffer[4] = (byte)(chunkSize);
            Array.Copy(Data, offset, buffer, 5, chunkSize);

            offset += chunkSize;
            return buffer;
        }
        #endregion
    }
}
