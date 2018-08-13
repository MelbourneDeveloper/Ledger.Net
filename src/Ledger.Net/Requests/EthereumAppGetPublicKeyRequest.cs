namespace Ledger.Net.Requests
{
    public class EthereumAppGetPublicKeyRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => (byte)(Display ? 1 : 0);
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.ETHEREUM_GET_WALLET_PUBLIC_KEY;
        #endregion

        #region Public Properties
        public bool Display { get; }
        #endregion

        #region Constructor
        public EthereumAppGetPublicKeyRequest(bool display, byte[] data) : base(data)
        {
            Display = display;
        }
        #endregion
    }
}
