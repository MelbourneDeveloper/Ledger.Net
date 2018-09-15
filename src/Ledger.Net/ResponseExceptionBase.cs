using System;

namespace Ledger.Net
{
    public class ResponseExceptionBase : Exception
    {
        public byte[] ResponseData { get; }

        public ResponseExceptionBase(string message, byte[] responseData) : base(message)
        {
            ResponseData = responseData;
        }
    }
}
