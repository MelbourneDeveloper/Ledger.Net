namespace Ledger.Net.Requests
{
    public class EthereumAppSignTransactionRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => 0;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => SignTransaction ? Constants.ETHEREUM_SIGN_TX : Constants.ETHEREUM_SIGN_MESSAGE;
        #endregion

        #region Public Properties
        public bool SignTransaction { get; }
        #endregion

        #region Constructor
        public EthereumAppSignTransactionRequest(bool signTransaction, byte[] data) : base(data)
        {
            SignTransaction = signTransaction;
        }
        #endregion
    }
}
