namespace Ledger.Net.Requests
{
    public class TronAppGetPublicKeyRequest : RequestBase
    {
        #region Public Overrides
        //From test_getPublicKey.py
        public override byte Argument1 => (byte)(Display ? 1 : 0);
        public override byte Argument2 => 0x00;
        public override byte Cla => Constants.CLA;
        public override byte Ins => 0x02;
        #endregion

        #region Public Properties
        public bool Display { get; }
        public BitcoinAddressType BitcoinAddressType { get; }
        #endregion

        #region Constructor
        public TronAppGetPublicKeyRequest(bool display, byte[] data) : base(data)
        {
            Display = display;
        }
        #endregion
    }
}
