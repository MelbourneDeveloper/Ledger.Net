namespace Ledger.Net
{
    public abstract class RequestBase
    {
        #region Public Abstract Properties
        public abstract byte Argument1 { get; }
        public abstract byte Argument2 { get; }
        public abstract byte Cla { get; }
        public abstract byte Ins { get; }
        #endregion

        #region Public Properties
        public byte[] Data { get; }
        #endregion

        public RequestBase()
        {
        }
    }
}
