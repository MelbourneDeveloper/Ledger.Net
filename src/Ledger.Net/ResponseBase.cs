namespace Ledger.Net
{
    public abstract class ResponseBase
    {
        #region Constants
        private const int HardeningConstant = 0xff;
        #endregion

        #region Public Properties
        public byte[] Data { get; }
        public int StatusCode { get; }
        public bool IsSuccess => ReturnCode == 0x9000;
        public int ReturnCode { get;  }
        #endregion

        #region Constructor
        public ResponseBase(byte[] data)
        {
            Data = data;
            ReturnCode = ((data[data.Length - 2] & HardeningConstant) << 8) | data[data.Length - 1] & HardeningConstant;
        }
        #endregion
    }
}
