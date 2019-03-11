namespace Ledger.Net.Exceptions
{
    public class IncorrectLengthException : ResponseBaseException
    {
        public IncorrectLengthException(byte[] responseData) : base("Incorrect length exception occurred. The Ledger received incorrect data. This probably means that there is no app loaded.", responseData)
        {
        }
    }
}
