using System;

namespace Ledger.Net
{
    public class SecurityException : Exception
    {
        public byte[] Data { get; }

        public SecurityException(byte[] data) : base("A security exception occurred. This probably means that the user has not entered their pin, or the current app does not support this instruction.")
        {
            Data = data;
        }
    }
}
