using System;

namespace Ledger.Net.Exceptions
{
    public class TooManyPromptsException : Exception
    {
        public int Attempts { get; }
        public string Member { get; }

        public TooManyPromptsException(int attempts, string member) : base($"User was prompted {attempts} times but the call {member} was not successful.")
        {
            Attempts = attempts;
            Member = member;
        }
    }
}
