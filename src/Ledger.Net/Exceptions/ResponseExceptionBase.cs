using System;

namespace Ledger.Net.Exceptions
{
    public abstract class ResponseExceptionBase : Exception
    {
        public byte[] ResponseData { get; }

        public ResponseExceptionBase(string message, byte[] responseData) : base(message)
        {
            ResponseData = responseData;
        }
    }
}
