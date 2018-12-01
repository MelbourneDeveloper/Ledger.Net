using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net.Exceptions
{
    public class InvalidAPDUResponseException : ResponseExceptionBase
    {
        public InvalidAPDUResponseException(string message, byte[] responseData) : base(message, responseData)
        {
        }
    }
}
