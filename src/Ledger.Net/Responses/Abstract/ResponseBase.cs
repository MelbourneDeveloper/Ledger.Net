﻿using Ledger.Net.Exceptions;

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
                    case Constants.InstructionNotSupportedStatusCode:
                        return "Instruction not supported in current app or there is no app running";
                    case 0x6B00:
                        return "Invalid parameter";
                    case 0x6A80:
                        return "The data is invalid";
                    case 0x6804:
                        return "Unknown error. Possibly from Firmware?";
                    case 0x6E00:
                        return "CLA not supported in current app";
                    case Constants.IncorrectLengthStatusCode:
                        return "Data length is incorrect";
                    case Constants.SecurityNotValidStatusCode:
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
            if (data == null || data.Length < 2)
                throw new InvalidAPDUResponseException(
                    string.Format("Invalid APDU response. Data lenth greater than 2 is expected, received {0}", data != null ? data.Length : 0),
                    data);
            Data = data;
            ReturnCode = ((data[data.Length - 2] & HardeningConstant) << 8) | data[data.Length - 1] & HardeningConstant;
        }
        #endregion
    }
}
