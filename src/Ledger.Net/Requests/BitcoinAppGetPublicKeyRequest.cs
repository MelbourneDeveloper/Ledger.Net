namespace Ledger.Net.Requests
{
    public class BitcoinAppGetPublicKeyRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => (byte)(Display ? 1 : 0);
        public override byte Argument2 => (byte)BitcoinAddressType;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.BTCHIP_INS_GET_WALLET_PUBLIC_KEY;
        #endregion

        #region Public Properties
        public bool Display { get; }
        public BitcoinAddressType BitcoinAddressType { get; }
        #endregion

        #region Constructor
        public BitcoinAppGetPublicKeyRequest(bool display, BitcoinAddressType bitcoinAddressType, byte[] data) : base(data)
        {
            Display = display;
            BitcoinAddressType = bitcoinAddressType;
        }
        #endregion
    }
}
