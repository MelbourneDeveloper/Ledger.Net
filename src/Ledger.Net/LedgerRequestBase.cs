namespace Ledger.Net
{
    public abstract class LedgerRequestBase
    {
        #region Public Abstract Properties
        public abstract object Argument1 { get; }
        public abstract object Argument2 { get; }
        public abstract byte Cla { get; }
        public abstract byte Ins { get; }
        #endregion

        #region Public Properties
        #endregion

        public LedgerRequestBase()
        {
        }
    }
}
