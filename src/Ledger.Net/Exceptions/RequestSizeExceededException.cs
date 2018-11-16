using Ledger.Net.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ledger.Net.Exceptions
{
    public class RequestSizeExceededException : Exception
    {
        public RequestBase Request { get; }

        public RequestSizeExceededException(RequestBase request) : base("The apdu request exeeds the maximun allowed size. Use chunked commands instead. ")
        {
            Request = request;
        }
    }
}
