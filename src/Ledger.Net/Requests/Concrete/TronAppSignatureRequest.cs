namespace Ledger.Net.Requests
{
    public class TronAppSignatureRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => 0;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins =>  Constants.TRON_SIGN_TX ;
        #endregion

        #region Public Properties
        public bool SignTransaction { get; }
        #endregion

        #region Constructor
        public TronAppSignatureRequest(byte[] data) : base(data)
        {
        }
        #endregion
    }
}
