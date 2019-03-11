namespace Ledger.Net.Exceptions
{
    public class SecurityException : ResponseBaseException
    {
        public SecurityException(byte[] responseData) : base("A security exception occurred. This probably means that the user has not entered their pin, or there is no app loaded.", responseData)
        {
        }
    }
}
