namespace Ledger.Net.Requests
{
    public class TronAppSignatureRequest : RequestBase
    {
        #region Public Overrides
        //TODO: This is wrong! This needs a rethink. This parameter is used to tell the ledger whether more data is coming or not. This probably needs to be removed from the base class
        public override byte Argument1 => Constants.P1_SIGN;
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
