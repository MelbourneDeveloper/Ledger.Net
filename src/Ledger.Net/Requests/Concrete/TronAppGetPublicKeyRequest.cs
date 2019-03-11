namespace Ledger.Net.Requests
{
    public class TronAppGetPublicKeyRequest : RequestBase
    {
        #region Public Overrides
        //From test_getPublicKey.py
        public override byte Argument1 => 0x00;
        public override byte Argument2 => 0x00;
        public override byte Cla => Constants.CLA;
        public override byte Ins => 0x02;
        #endregion

        #region Constructor
        public TronAppGetPublicKeyRequest(byte[] data) : base(data)
        {
        }
        #endregion
    }
}
