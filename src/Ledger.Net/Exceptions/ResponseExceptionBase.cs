using System;

namespace Ledger.Net.Exceptions
{
    public abstract class ResponseExceptionBase : Exception
    {
        public byte[] ResponseData { get; }

        protected ResponseExceptionBase(string message, byte[] responseData) : base(message)
        {
            ResponseData = responseData;
        }
    }
}
