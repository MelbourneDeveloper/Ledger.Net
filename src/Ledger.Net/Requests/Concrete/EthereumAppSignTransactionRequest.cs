namespace Ledger.Net.Requests
{
    public class EthereumAppSignTransactionRequest : RequestBase
    {
        private const byte P1_FIRST_TRANSACTION_BLOCK = 0x00;
        private const byte P1_SUBSEQUENT_TRANSACTION_BLOCK = 0x80;

        #region Public Overrides
        public override byte Argument1 => FirstTransactionDataBlock ? P1_FIRST_TRANSACTION_BLOCK : P1_SUBSEQUENT_TRANSACTION_BLOCK;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.ETHEREUM_SIGN_TX;
        #endregion

        #region Public Properties
        public bool FirstTransactionDataBlock { get; }
        #endregion

        #region Constructor
        public EthereumAppSignTransactionRequest(bool firstTransactionDataBlock, byte[] data) : base(data)
        {
            FirstTransactionDataBlock = firstTransactionDataBlock;
        }
        #endregion
    }
}
