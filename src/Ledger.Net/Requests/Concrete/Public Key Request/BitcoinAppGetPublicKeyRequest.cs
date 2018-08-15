namespace Ledger.Net.Requests
{
    public class BitcoinAppGetPublicKeyRequest : GetPublicKeyRequestBase
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
        public BitcoinAppGetPublicKeyRequest(uint index, bool display) : base(0, 0, index, false, true)
        {
            Display = display;
            BitcoinAddressType = BitcoinAddressType.Segwit;
        }

        public BitcoinAppGetPublicKeyRequest(
            uint coinNumber,
            uint account,
            uint index,
            bool isSegwit,
            bool isChange,
            bool display) : base(coinNumber, account, index, isChange, isSegwit)
        {
            Display = display;
            BitcoinAddressType = isSegwit ? BitcoinAddressType.Segwit : BitcoinAddressType.Legacy;
        }
        #endregion
    }
}
