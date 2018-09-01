namespace Ledger.Net.Requests
{
    public class EthereumAppSignTransactionRequest : RequestBase
    {
        // The first block of data that is sent needs to be 0x00, and the rest would be 0x80. How should this be done?
        // Javascript implementation: https://github.com/LedgerHQ/ledgerjs/blob/master/packages/hw-app-eth/src/Eth.js#L147
        // Foreach function uses 0x00 on the first index of the data array and 0x80 on the rest.
        // Foreach implementation is moved to the utils.js file: https://github.com/LedgerHQ/ledgerjs/blob/master/packages/hw-app-eth/src/utils.js#L58

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
