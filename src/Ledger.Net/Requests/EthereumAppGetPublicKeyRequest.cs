namespace Ledger.Net.Requests
{
    public class EthereumAppGetPublicKeyRequest : RequestBase
    {
        #region Constants
        private const byte P2_NO_CHAINCODE = 0x00;
        private const byte P2_CHAINCODE = 0x01;
        private const byte P1_CONFIRM = 0x01;
        private const byte P1_NON_CONFIRM = 0x00;
        #endregion

        #region Public Overrides
        public override byte Argument1 => Display ? P1_CONFIRM : P1_NON_CONFIRM;
        public override byte Argument2 => UseChainCode ? P2_CHAINCODE : P2_NO_CHAINCODE;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.ETHEREUM_GET_WALLET_PUBLIC_KEY;
        #endregion

        #region Public Properties
        public bool Display { get; }
        public bool UseChainCode { get; }
        #endregion

        #region Constructor
        public EthereumAppGetPublicKeyRequest(bool display, bool useChainCode, byte[] data) : base(data)
        {
            Display = display;
            UseChainCode = useChainCode;
        }
        #endregion
    }
}
