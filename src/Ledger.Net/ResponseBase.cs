namespace Ledger.Net
{
    public abstract class ResponseBase
    {
        #region Public Properties
        public byte[] Data { get; }
        #endregion

        #region Constructor
        public ResponseBase(byte[] data)
        {
            Data = data;
        }
        #endregion
    }
}
