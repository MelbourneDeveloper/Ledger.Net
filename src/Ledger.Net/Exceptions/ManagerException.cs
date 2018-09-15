using System;

namespace Ledger.Net.Exceptions
{
    public class ManagerException : Exception
    {
        public ManagerException(string message) : base(message)
        {
        }
    }
}
