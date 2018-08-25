namespace Ledger.Net.Requests
{
    public class GetCoinVersionRequest : RequestBase
    {
        #region Public Overrides
        public override byte Argument1 => 0;
        public override byte Argument2 => 0;
        public override byte Cla => Constants.CLA;
        public override byte Ins => Constants.BTCHIP_INS_GET_COIN_VER;
        #endregion

        #region Constructor
        public GetCoinVersionRequest() : base(new byte[0])
        {
        }
        #endregion
    }
}
