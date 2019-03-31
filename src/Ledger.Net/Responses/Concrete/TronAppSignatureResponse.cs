namespace Ledger.Net.Responses
{
    public class TronAppSignatureResponse : ResponseBase
    {
        public TronAppSignatureResponse(byte[] data) : base(data)
        {
            if (!IsSuccess)
            {
                return;
            }
        }
    }
}
