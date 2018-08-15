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
        public int ReturnCode { get; }
        public string StatusMessage
        {
            get
            {
                switch (ReturnCode)
                {
                    case 0x9000:
                        return "Success";
                    case 0x6D00:
                        return "Instruction not supported in current app or there is no app running";
                    case 0x6B00:
                        return "Invalid parameter";
                    default:
                        return "Shrugging in your general direction";
                }
            }
        }
        #endregion

        private int asdasd =  0x6804;

        #region Constructor
        public ResponseBase(byte[] data)
        {
            Data = data;
            ReturnCode = ((data[data.Length - 2] & HardeningConstant) << 8) | data[data.Length - 1] & HardeningConstant;
        }
        #endregion
    }
}
