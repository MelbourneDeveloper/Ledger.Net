namespace Ledger.Net.Requests
{
    public class EthereumAppSignTransactionRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => 0;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.ETHEREUM_SIGN_TX;
        #endregion

        #region Constructor
        public EthereumAppSignTransactionRequest(byte[] data) : base(data)
        {
        }
        #endregion
    }
}
