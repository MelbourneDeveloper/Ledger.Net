namespace Ledger.Net.Responses
{
    public abstract class ResponseBase
    {
        #region Constants
        private const int HardeningConstant = 0xff;
        #endregion

        #region Public Properties
        public byte[] Data { get; }
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
                    case 0x6A80:
                        return "The data is invalid";
                    case 0x6804:
                        return "Unknown error. Possibly from Firmware?";
                    case 0x6E00:
                        return "CLA not supported in current app";
                    case 0x6700:
                        return "Data length is incorrect?";
                    case 0x6982:
                        return "The security is not valid for this command";
                    case 0x6985:
                        return "Conditions have not been satisfied for this command";
                    case 0x6482:
                        return "File not found";
                    default:
                        return "Shrugging in your general direction";
                }
            }
        }
        #endregion

        #region Constructor
        protected ResponseBase(byte[] data)
        {
            Data = data;
            ReturnCode = ((data[data.Length - 2] & HardeningConstant) << 8) | data[data.Length - 1] & HardeningConstant;
        }
        #endregion
    }
}
